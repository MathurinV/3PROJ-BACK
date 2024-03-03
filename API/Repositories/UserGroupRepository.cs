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

    public Task<bool> IsUserInGroup(Guid userId, Guid groupId)
    {
        return context.UserGroups.AnyAsync(x => x.GroupId == groupId && x.UserId == userId);
    }

    public Task<bool> AreUsersInGroup(Guid groupId, IEnumerable<Guid> userIds)
    {
        return context.UserGroups
            .Where(x => x.GroupId == groupId)
            .Select(x => x.UserId)
            .AllAsync(x => userIds.Contains(x));
    }
}