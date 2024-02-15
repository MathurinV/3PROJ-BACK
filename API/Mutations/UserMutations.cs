using DAL.Models.UserGroups;
using DAL.Models.Users;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class UserMutations
{
    public async Task<AppUser?> CreateUser([FromServices] IUserRepository userRepository,
        AppUserInsertDto appUserInsertDto)
    {
        return await userRepository.InsertAsync(appUserInsertDto);
    }

    public async Task<SignInResult> SignIn([FromServices] IUserRepository userRepository,
        AppUserLoginDto appUserLoginDto)
    {
        return await userRepository.SignInAsync(appUserLoginDto);
    }
    
    [Authorize]
    public async Task<bool> SIgnOut([FromServices] IUserRepository userRepository)
    {
        return await userRepository.SIgnOutAsync();
    }
    
    [Authorize]
    public async Task<UserGroup?> JoinGroup([FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IInvitationRepository invitationRepository,
        [FromServices] IGroupRepository groupRepository,
        UserGroupInsertDto userGroupInsertDto)
    {
        if (!await invitationRepository.DeleteAsync(userGroupInsertDto.GroupId, userGroupInsertDto.UserId)) return null;
        return await userGroupRepository.InsertAsync(userGroupInsertDto);
    }
}