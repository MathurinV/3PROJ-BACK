using System.Security.Claims;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.UserGroups;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

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

    [Authorize]
    public async Task<string> UploadGroupImagePicture(Guid groupId,
        [FromServices] IUserRepository userRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IDistributedCache distributedCache)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        var group = await groupRepository.GetByIdAsync(groupId) ?? throw new Exception("Group not found");
        var user = await userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");

        if (group.OwnerId != user.Id) throw new Exception("You are not the owner of this group");

        var token = Guid.NewGuid().ToString();
        await distributedCache.SetStringAsync(token, groupId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        var baseUrl = $"http://localhost:{DockerEnv.ApiPort}";

        return $"{baseUrl}/groupimages/{token}";
    }
    
    [Authorize]
    public async Task<Group?> ModifyGroup([FromServices] IGroupRepository groupRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        GroupModifyDto groupModifyDto)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        var group = await groupRepository.GetByIdAsync(groupModifyDto.GroupId);
        if (group == null) throw new Exception("Group not found");
        return await groupRepository.ModifyAsync(Guid.Parse(userId), groupModifyDto);
    }
}