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
    public async Task<ICollection<UserExpense?>> AddUserExpense(
        ExpenseInsertInput expenseInsertInput,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           throw new Exception("User not found");

        var userId = Guid.Parse(userIdString);
        var user = await userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");

        var guidAmountsList = expenseInsertInput.UsersWithAmount;

        var expenseInsertDto = expenseInsertInput.ToExpenseInsertDto(userId);

        var userIds = guidAmountsList.Select(x => x.Key).ToList();
        if (!await userGroupRepository.AreUsersInGroup(expenseInsertDto.GroupId, userIds))
            throw new Exception("All users are not in the group");

        var totalAmount = guidAmountsList.Sum(x => x.Value);
        expenseInsertDto.Amount = totalAmount;
        var currentExpense = await expenseRepository.InsertAsync(expenseInsertDto) ??
                             throw new Exception("Failed to insert expense");

        var userExpenses = new List<UserExpenseInsertDto>();
        foreach (var (guid, amount) in guidAmountsList)
            userExpenses.Add(new UserExpenseInsertDto
            {
                ExpenseId = currentExpense.Id,
                UserId = guid,
                Amount = amount
            });

        return await userExpenseRepository.InsertManyAsync(userExpenses);
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