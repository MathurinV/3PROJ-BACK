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
        GroupInsertDto groupInsertDto)
    {
        var currentGroup = await groupRepository.InsertAsync(groupInsertDto);
        if (currentGroup == null) return null;
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
        InvitationInsertDto invitationInsertDto)
    {
        var group = await groupRepository.GetByIdAsync(invitationInsertDto.GroupId);
        if (group == null) return null;
        var user = userRepository.GetById(invitationInsertDto.UserId);
        if (await user.FirstAsync() == null) return null;
        return await invitationRepository.InsertAsync(invitationInsertDto);
    }

    [Authorize]
    public async Task<ICollection<UserExpense?>> AddUserExpenses(
        [FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IExpenseRepository expenseRepository,
        ExpenseInsertDto expenseInsertDto
    )
    {
        var expense = await expenseRepository.InsertAsync(expenseInsertDto);
        if (expense == null) return [];
        var totalWeight = expenseInsertDto.WeightedUsers.Sum(x => x.Value);
        var userExpenses = new List<UserExpenseInsertDto>();

        foreach (var (userId, weight) in expenseInsertDto.WeightedUsers)
            userExpenses.Add(new UserExpenseInsertDto
            {
                ExpenseId = expense.Id,
                UserId = userId,
                Amount = expense.Amount * weight / totalWeight
            });
        return await userExpenseRepository.InsertManyAsync(userExpenses);
    }
}