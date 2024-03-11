using DAL.Models.UserGroups;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class UserGroupRepository(MoneyMinderDbContext context) : IUserGroupRepository
{
    public async Task<UserGroup?> InsertAsync(UserGroupInsertDto userGroupInsertDto)
    {
        var userGroup = userGroupInsertDto.ToUserGroup();
        await context.UserGroups.AddAsync(userGroup);
        await context.SaveChangesAsync();
        return userGroup;
    }

    public async Task<bool> IsUserInGroup(Guid userId, Guid groupId)
    {
        return await context.UserGroups.AnyAsync(x => x.GroupId == groupId && x.UserId == userId);
    }

    public async Task<bool> AreUsersInGroup(Guid groupId, IEnumerable<Guid> userIds)
    {
        var userGroups = await context.UserGroups.Where(x => x.GroupId == groupId).ToListAsync();
        return userIds.All(userId => userGroups.Any(x => x.UserId == userId));
    }
}