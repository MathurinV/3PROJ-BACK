using System.Security.Claims;
using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class GroupMutations
{
    [Authorize]
    public async Task<Group?> CreateGroup([FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        GroupInsertInput groupInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        Console.WriteLine(userId);
        var currentGroup = await groupRepository.InsertAsync(groupInsertInput.ToGroupInsertDto(Guid.Parse(userId)));
        if (currentGroup == null) throw new Exception("Group not created");
        var userGroup = new UserGroupInsertDto
        {
            UserId = currentGroup.OwnerId,
            GroupId = currentGroup.Id
        };
        await userGroupRepository.InsertAsync(userGroup);
        return groupRepository.GetByIdAsync(currentGroup.Id).Result;
    }

    [Authorize]
    public async Task<Invitation?> InviteUser([FromServices] IInvitationRepository invitationRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        InvitationInsertDto invitationInsertDto)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        var group = await groupRepository.GetByIdAsync(invitationInsertDto.GroupId);
        if (group == null) throw new Exception("Group not found");
        var user = userRepository.GetById(invitationInsertDto.UserId);
        if (await user.FirstAsync() == null) throw new Exception("User not found");
        if (Guid.Parse(userId) == invitationInsertDto.UserId) throw new Exception("You can't invite yourself ???");

        return await invitationRepository.InsertAsync(invitationInsertDto);
    }

    [Authorize]
    public async Task<ICollection<UserExpense?>> AddUserExpenses(
        [FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        ExpenseInsertInput expenseInsertInput
    )
    {
        var createdByStringId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Cheking if the logged in user exists
        if (createdByStringId == null) throw new Exception("User not found");

        var createdByGuidId = Guid.Parse(createdByStringId);
        var expenseInsertDto = expenseInsertInput.ToExpenseInsertDto(createdByGuidId);

        // Checks if all the weighted users are in the group
        var weightedUsers = expenseInsertInput.WeightedUsers.Select(x => x.Key);
        var currentGroup = await groupRepository.GetByIdAsync(expenseInsertInput.GroupId);
        if (currentGroup == null) throw new Exception("Group not found");
        if (userGroupRepository.AreUsersInGroup(currentGroup.Id, weightedUsers).Result == false)
            throw new Exception("One or more users are not in the group");

        var totalWeight = expenseInsertInput.WeightedUsers.Sum(x => x.Value);
        var totalAmount = expenseInsertInput.Amount;

        // Handle the case where the creator is also a weighted user
        var creatorWeightedUser = expenseInsertDto.WeightedUsers.FirstOrDefault(x => x.Key == createdByGuidId);
        if (creatorWeightedUser.Key != default)
        {
            // The creator is also a weighted user, we need to remove him from the weighted users and adjust the total amount
            // In this case, the creator has been checked to be in the group
            totalAmount -= expenseInsertDto.Amount * creatorWeightedUser.Value / totalWeight;
            totalWeight -= creatorWeightedUser.Value;
            expenseInsertDto.WeightedUsers.Remove(creatorWeightedUser);
        }
        else
        {
            // the creator is not a weighted user, we need to check if he is in the group
            if (userGroupRepository.IsUserInGroup(createdByGuidId, currentGroup.Id).Result == false)
                throw new Exception("The creator is not in the group");
        }

        await using var transaction = await expenseRepository.BeginTransactionAsync();
        try
        {
            var expense = await expenseRepository.InsertAsync(expenseInsertDto);
            if (expense == null) return [];

            var userExpenses = new List<UserExpenseInsertDto>();

            foreach (var (userId, userWeight) in expenseInsertDto.WeightedUsers)
            {
                // If the creator is also a weighted user, we don't want to add an expense for him
                if (userId == createdByGuidId) continue;

                var amountDue = totalAmount * userWeight / totalWeight;
                if (Math.Round(amountDue, 2) != amountDue)
                    throw new Exception(
                        $"The amount due for user {userId} is not a multiple of 0.01 (got {amountDue})");
                userExpenses.Add(new UserExpenseInsertDto
                {
                    ExpenseId = expense.Id,
                    UserId = userId,
                    Amount = amountDue
                });
            }

            var insertedUserExpenses = await userExpenseRepository.InsertManyAsync(userExpenses);

            await transaction.CommitAsync();

            return insertedUserExpenses;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}