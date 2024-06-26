using System.Security.Claims;
using API.ErrorsHandling.UsersHandling;
using DAL.Models.Expenses;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using DAL.Repositories;
using FluentFTP;
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

    [Authorize]
    public async Task<AppUser?> ModifyMyself([FromServices] IUserRepository userRepository,
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        AppUserModifyDto appUserModifyDto)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        AppUserModifyDtoHandling.ValidateAppUserModifyDto(userManager, appUserModifyDto);
        if (appUserModifyDto.Password != null)
            await userRepository.ChangeMyPasswordAsync(Guid.Parse(userId), appUserModifyDto);

        if (appUserModifyDto.UserName != null || appUserModifyDto.Email != null)
            return await userRepository.ModifyAsync(Guid.Parse(userId), appUserModifyDto);

        return userRepository.GetById(Guid.Parse(userId)).FirstOrDefault();
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
        return result;
    }

    [Authorize]
    public async Task<UserGroup?> JoinGroup([FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IInvitationRepository invitationRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IPayDueToRepository payDueToRepository,
        UserGroupInsertInput userGroupInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        var userGroupInsertDto = userGroupInsertInput.ToUserGroupInsertDto(Guid.Parse(userId));
        if (!await invitationRepository.DeleteAsync(userGroupInsertDto.GroupId, userGroupInsertDto.UserId))
            throw new Exception("Invitation not found");
        if (await userGroupRepository.IsUserInGroup(userGroupInsertDto.UserId, userGroupInsertDto.GroupId))
            throw new Exception("User is already in the group");
        var tmp = await userGroupRepository.InsertAsync(userGroupInsertDto);
        await payDueToRepository.InitPayDueToAsync(userGroupInsertDto.GroupId, userGroupInsertDto.UserId);
        return tmp;
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
    public async Task<string> UploadUserRib([FromServices] IUserRepository userRepository,
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

        return $"{baseUrl}/ribs/{token}";
    }

    [Authorize]
    public async Task<string> GetUserRib(Guid ribUserId,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IUserRepository userRepository,
        [FromServices] IDistributedCache distributedCache
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        var currentUser = await userRepository.GetByIdAsync(userId) ??
                          throw new Exception("User not found");
        var otherUser = await userRepository.GetByIdAsync(ribUserId) ??
                        throw new Exception("Other user not found");

        var isCurrentUserDueToOtherUser = await userRepository.IsDueToAsync(currentUser.Id, otherUser.Id);
        if (!isCurrentUserDueToOtherUser && userId != ribUserId)
            throw new Exception("No money has to be paid to this user");

        var fileExtension = JustificationFileTypes.ValidJustificationExtensionToString(otherUser.RibExtension);

        var ftpClient = new AsyncFtpClient("ftp", DockerEnv.FtpUserRibsUser, DockerEnv.FtpUserRibsPassword);
        await ftpClient.AutoConnect();
        if (!await ftpClient.FileExists($"{ribUserId}{fileExtension}")) throw new Exception("Rib not found");
        await ftpClient.Disconnect();

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, ribUserId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";
        return $"{baseUrl}/ribs/{token}";
    }
    
    [Authorize]
    public async Task<string> GetUserInfo([FromServices] IUserRepository userRepository,
        [FromServices] IDistributedCache distributedCache,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        var user = await userRepository.GetByIdAsync(userId) ??
                   throw new Exception("User not found");

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, userIdString, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        
        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";
        return $"{baseUrl}/usersumups/{token}";
    }
}