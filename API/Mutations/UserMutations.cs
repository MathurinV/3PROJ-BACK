using System.Security.Claims;
using API.ErrorsHandling.UsersHandling;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        return await userRepository.InsertAsync(appUserInsertDto);
    }

    public async Task<SignInResult> SignIn([FromServices] IUserRepository userRepository,
        [FromServices] UserManager<AppUser> userManager,
        AppUserLoginDto appUserLoginDto)
    {
        AppUserLoginDtoHandling.ValidateAppUserLoginDto(userManager, appUserLoginDto);
        return await userRepository.SignInAsync(appUserLoginDto);
    }

    [Authorize]
    public async Task<bool> SignOut([FromServices] IUserRepository userRepository)
    {
        return await userRepository.SignOutAsync();
    }

    [Authorize]
    public async Task<UserGroup?> JoinGroup([FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IInvitationRepository invitationRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        UserGroupInsertInput userGroupInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("User not found");
        var userGroupInsertDto = userGroupInsertInput.ToUserGroupInsertDto(Guid.Parse(userId));
        if (!await invitationRepository.DeleteAsync(userGroupInsertDto.GroupId, userGroupInsertDto.UserId))
            throw new Exception("Invitation not found");
        return await userGroupRepository.InsertAsync(userGroupInsertDto);
    }

    [Authorize]
    public async Task<bool> PayDues([FromServices] IUserExpenseRepository userExpenseRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) throw new Exception("User not found");
        
        return await userExpenseRepository.PayByUserIdAsync(Guid.Parse(userIdString));
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