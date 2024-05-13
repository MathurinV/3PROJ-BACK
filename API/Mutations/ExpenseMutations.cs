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
    public async Task<ICollection<UserExpense>> AddUserExpense(
        ExpenseInsertInput expenseInsertInput,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IPayDueToRepository payDueToRepository
    )
    {
        var creatorStringId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              throw new Exception("User not found");
        var creatorId = Guid.Parse(creatorStringId);
        var creator = await userRepository.GetByIdAsync(creatorId) ?? throw new Exception("User not found");

        var group = await groupRepository.GetByIdAsync(expenseInsertInput.GroupId) ??
                    throw new Exception("Group not found");

        var userAmountsList = expenseInsertInput.UserAmountsList;

        // Checks if all the weighted users are in the group
        if (!await userGroupRepository.AreUsersInGroup(expenseInsertInput.GroupId,
                userAmountsList.Select(x => x.Key)))
            throw new Exception("Not all the weighted users are in the group");

        // var creatorInUserAmountsList =
        //     userAmountsList.FirstOrDefault(x => x.Key == creatorId);

        var totalAmount = Math.Round(expenseInsertInput.Amount, 2);
        var numberOfUsersWithoutSpecifiedAmount = 0;
        var userIdsWithoutSpecifiedAmount = new List<Guid>();

        var userIdsWithAmounts = new List<KeyValuePair<Guid, decimal>>();

        foreach (var keyValuePair in userAmountsList)
            if (keyValuePair.Value.HasValue)
            {
                var currentUserAmount = Math.Round(keyValuePair.Value.Value, 2);
                userIdsWithAmounts.Add(new KeyValuePair<Guid, decimal>(keyValuePair.Key, currentUserAmount));
                totalAmount -= currentUserAmount;
                if (totalAmount < 0)
                    throw new Exception("The sum of all the users' amounts is greater than the total amount");
            }
            else
            {
                numberOfUsersWithoutSpecifiedAmount += 1;
                userIdsWithoutSpecifiedAmount.Add(keyValuePair.Key);
            }

        var newTotalAmount = decimal.Zero;

        // At this point, the list of users has been explored, and we know the amount left to be paid by the users without a specified amount
        if (numberOfUsersWithoutSpecifiedAmount > 0)
        {
            var toBePaidByUserAmount = Math.Round(totalAmount / numberOfUsersWithoutSpecifiedAmount, 2);
            foreach (var id in userIdsWithoutSpecifiedAmount)
            {
                userIdsWithAmounts.Add(new KeyValuePair<Guid, decimal>(id, toBePaidByUserAmount));
            }
        }

        newTotalAmount = userIdsWithAmounts.Sum(x => x.Value);

        var expenseInsertDto = expenseInsertInput.ToExpenseInsertDto(creatorId);

        var userIds = userIdsWithAmounts.Select(x => x.Key).ToList();
        if (!await userGroupRepository.AreUsersInGroup(expenseInsertDto.GroupId, userIds))
            throw new Exception("All users are not in the group");

        expenseInsertDto.Amount = newTotalAmount;
        var currentExpense = await expenseRepository.InsertAsync(expenseInsertDto) ??
                             throw new Exception("Failed to insert expense");

        var userExpenses = new List<UserExpenseInsertDto>();
        foreach (var (guid, amount) in userIdsWithAmounts)
            userExpenses.Add(new UserExpenseInsertDto
            {
                ExpenseId = currentExpense.Id,
                UserId = guid,
                Amount = amount
            });

        var res = await userExpenseRepository.InsertManyAsync(userExpenses);
        await payDueToRepository.RefreshPayDueTosAsync(expenseInsertInput.GroupId);
        return res;
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