using DAL.Models.Groups;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Queries;

[ExtendObjectType("Query")]
public class GroupQueries
{
    public async Task<IEnumerable<Group>> GetGroups([FromServices] IGroupRepository groupRepository)
    {
        return await groupRepository.GetAllAsync();
    }

    public async Task<Group?> GetGroup([FromServices] IGroupRepository groupRepository, Guid id)
    {
        return await groupRepository.GetByIdAsync(id);
    }
}