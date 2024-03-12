using System.Security.Claims;
using DAL.Models.Users;
using DAL.Repositories;
using FluentFTP;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Queries;

[ExtendObjectType("Query")]
public class UserQueries
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser> GetUsers([Service] IUserRepository userRepository)
    {
        return userRepository.GetAll();
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetCurrentUser([Service] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("Issue with getting user id");
        return userRepository.GetById(Guid.Parse(userId));
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetUserById([Service] IUserRepository userRepository, Guid id)
    {
        return userRepository.GetById(id);
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetUserByEmail([Service] IUserRepository userRepository, string email = null!)
    {
        return userRepository.GetByEmail(email);
    }

    [Authorize]
    public async Task<string> GetExpenseJustification(Guid expenseId,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IUserRepository userRepository,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IDistributedCache distributedCache)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null) throw new Exception("Expense not found");

        var user = await userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null) throw new Exception("User not found");
        if (expense.CreatedById != user.Id) throw new Exception("You are not the creator of this expense");

        var ftpCLient = new AsyncFtpClient("ftp", DockerEnv.FtpUser, DockerEnv.FtpPassword);
        await ftpCLient.AutoConnect();
        if (!await ftpCLient.FileExists($"/justifications/{expenseId}")) throw new Exception("Justification not found");
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