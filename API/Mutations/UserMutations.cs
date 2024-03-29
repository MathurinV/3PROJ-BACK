using System.Security.Claims;
using API.ErrorsHandling.UsersHandling;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class UserMutations
{
    public async Task<AppUser?> CreateUser([FromServices] IUserRepository userRepository,
        [FromServices] UserManager<AppUser> userManager,
        AppUserInsertDto appUserInsertDto)
    {
        AppUserInsertDtoHandling.ValidateAppUserInsertDto(userManager, appUserInsertDto);
        var createdUser = await userRepository.InsertAsync(appUserInsertDto);
        await userRepository.SignInAsync(new AppUserLoginDto
        {
            Password = appUserInsertDto.Password,
            Username = appUserInsertDto.UserName,
            RememberMe = false
        });
        return createdUser;
    }

    public async Task<SignInResult> SignIn([FromServices] IUserRepository userRepository,
        [FromServices] UserManager<AppUser> userManager,
        AppUserLoginDto appUserLoginDto)
    {
        AppUserLoginDtoHandling.ValidateAppUserLoginDto(userManager, appUserLoginDto);
        var signInResult = await userRepository.SignInAsync(appUserLoginDto);
        if (signInResult.Succeeded == false) throw new Exception("Wrong username or password");

        return signInResult;
    }

    [Authorize]
    public async Task<bool> SignOut([FromServices] IUserRepository userRepository)
    {
        return await userRepository.SignOutAsync();
    }

    /// <summary>
    ///     Deletes the user record from the database asynchronously.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <returns>
    ///     A Task that represents the asynchronous operation. The task will be completed with a boolean value indicating
    ///     whether the user record was successfully deleted (true) or not (false).
    /// </returns>
    /// <exception cref="Exception">Thrown when the user is not found.</exception>
    /// <remarks>
    ///     The user must be authenticated to use this method. If the user is deleted, he is automatically signed out.
    /// </remarks>
    [Authorize]
    public async Task<bool> DeleteSelf([FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        var result = await userRepository.DeleteAsync(Guid.Parse(userId));
        if (result) await userRepository.SignOutAsync();
        return result;
    }

    [Authorize]
    public async Task<UserGroup?> JoinGroup([FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IInvitationRepository invitationRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        UserGroupInsertInput userGroupInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        var userGroupInsertDto = userGroupInsertInput.ToUserGroupInsertDto(Guid.Parse(userId));
        if (!await invitationRepository.DeleteAsync(userGroupInsertDto.GroupId, userGroupInsertDto.UserId))
            throw new Exception("Invitation not found");
        if (await userGroupRepository.IsUserInGroup(userGroupInsertDto.UserId, userGroupInsertDto.GroupId))
            throw new Exception("User is already in the group");
        return await userGroupRepository.InsertAsync(userGroupInsertDto);
    }

    [Authorize]
    public async Task<bool> RefuseInvitation([FromServices] IInvitationRepository invitationRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        Guid groupId)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        var user = userRepository.GetById(Guid.Parse(userId));
        if (user.FirstOrDefault() == null) throw new Exception("User not found");
        if (!await invitationRepository.DeleteAsync(groupId, Guid.Parse(userId)))
            throw new Exception("Invitation not found");
        return true;
    }


    [Authorize]
    public async Task<bool> PayDues([FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) throw new Exception("User not found");

        var expenseCreatorsAmountsPairs = await userExpenseRepository.PayDuesByUserIdAsync(Guid.Parse(userIdString));
        if (expenseCreatorsAmountsPairs.Count == 0) return false;
        await userRepository.AddToBalancesAsync(expenseCreatorsAmountsPairs);
        return true;
    }

    [Authorize]
    public async Task<bool> PayDuesByExpenseId([FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        ICollection<Guid> expenseIds)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) throw new Exception("User not found");

        var expenseCreatorsAmountsPairs =
            await userExpenseRepository.PayDuesByUserIdAndExpenseIdsAsync(Guid.Parse(userIdString), expenseIds);
        if (expenseCreatorsAmountsPairs.Count == 0) return false;
        await userRepository.AddToBalancesAsync(expenseCreatorsAmountsPairs);
        return true;
    }

    [Authorize]
    public async Task<string> UploadProfilePicture([FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IDistributedCache distributedCache)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, userIdString, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";

        return $"{baseUrl}/avatars/{token}";
    }

    [Authorize]
    public async Task<bool> AddToBalance([FromServices] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        decimal amount)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        return await userRepository.AddToBalanceAsync(Guid.Parse(userId), amount);
    }
}