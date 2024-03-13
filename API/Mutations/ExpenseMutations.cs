using System.Security.Claims;
using DAL.Models.Expenses;
using DAL.Models.UserExpenses;
using DAL.Repositories;
using FluentFTP;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class ExpenseMutations
{
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
        if (await userGroupRepository.AreUsersInGroup(expenseInsertDto.GroupId,
                expenseInsertDto.WeightedUsers.Select(x => x.Key)) == false)
            throw new Exception("Not all the weighted users are in the group");

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
            if (await userGroupRepository.IsUserInGroup(createdByGuidId, expenseInsertDto.GroupId) == false)
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

    [Authorize]
    public async Task<string> UploadExpenseJustification(Guid expenseId,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IDistributedCache distributedCache)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null) throw new Exception("Expense not found");

        var user = await userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null) throw new Exception("User not found");
        if (expense.CreatedById != user.Id) throw new Exception("You are not the creator of this expense");

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, expenseId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";

        return $"{baseUrl}/justifications/{token}";
    }

    [Authorize]
    public async Task<string> GetExpenseJustification(Guid expenseId,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IUserRepository userRepository,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IDistributedCache distributedCache,
        [FromServices] IUserGroupRepository userGroupRepository)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null) throw new Exception("Expense not found");

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var isUserInGroup = await userGroupRepository.IsUserInGroup(userId, expense.GroupId);
        if (!isUserInGroup) throw new Exception("You are not in the group of this expense");

        var fileExtension = JustificationFileTypes.ValidJustificationExtensionToString(expense.JustificationExtension);

        var ftpCLient = new AsyncFtpClient("ftp", DockerEnv.FtpJustificationsUser, DockerEnv.FtpJustificationsPassword);
        await ftpCLient.AutoConnect();
        if (!await ftpCLient.FileExists($"{expenseId}{fileExtension}")) throw new Exception("Justification not found");
        await ftpCLient.Disconnect();

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, expenseId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";
        return $"{baseUrl}/justifications/{token}";
    }
}