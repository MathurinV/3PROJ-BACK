using DAL.Models.Groups;
using DAL.Models.UserGroups;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class GroupMutations
{
    public async Task<Group?> CreateGroup([FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        GroupInsertDto groupInsertDto)
    {
        var currentGroup = await groupRepository.InsertAsync(groupInsertDto);
        if (currentGroup == null) return null;
        var userGroup = new UserGroupInsertDto
        {
            UserId = currentGroup.OwnerId,
            GroupId = currentGroup.Id
        };
        await userGroupRepository.InsertAsync(userGroup);
        return await groupRepository.GetByIdAsync(currentGroup.Id);
    }
    
    public async Task<Group?> JoinGroup([FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IGroupRepository groupRepository,
        UserGroupInsertDto userGroupInsertDto)
    {
        var userGroup = new UserGroupInsertDto
        {
            UserId = userGroupInsertDto.UserId,
            GroupId = userGroupInsertDto.GroupId
        };
        await userGroupRepository.InsertAsync(userGroup);
        return await groupRepository.GetByIdAsync(userGroupInsertDto.GroupId);
    }
}