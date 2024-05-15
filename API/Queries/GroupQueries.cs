using System.Security.Claims;
using DAL.Models.Groups;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Queries;

[ExtendObjectType("Query")]
public class GroupQueries
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Group> GetGroups([FromServices] IGroupRepository groupRepository)
    {
        return groupRepository.GetAll();
    }

    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Group?> GetGroupById([FromServices] IGroupRepository groupRepository, Guid id)
    {
        return groupRepository.GetById(id);
    }

    [Authorize]
    public async Task<ICollection<KeyValuePair<Guid, decimal>>> GetGroupBalances(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserRepository userRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        Guid groupId)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);

        if (await userRepository.GetByIdAsync(userId) == null)
            throw new Exception("User not found");

        if (!await userGroupRepository.IsUserInGroup(userId, groupId))
            throw new Exception("User is not in the group.");

        return await groupRepository.GetGroupBalances(groupId);
    }
}