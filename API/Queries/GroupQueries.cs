using DAL.Models.Groups;
using DAL.Repositories;
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

    // [Authorize]
    // [UseProjection]
    // [UseFiltering]
    // [UseSorting]
    // public ICollection<KeyValuePair<AppUser, decimal>> GetGroupBalances(
    //     [FromServices] IHttpContextAccessor httpContextAccessor,
    //     [FromServices] IGroupRepository groupRepository,
    //     [FromServices] IUserRepository userRepository,
    //     Guid groupId)
    // {
    //     
    //     return groupRepository.GetGroupBalances(groupId);
    // }
}