using System.Security.Claims;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.UserGroups;
using DAL.Repositories;
using HotChocolate.Authorization;
using HotChocolate.Language;
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
}