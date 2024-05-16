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

    public async Task AddToBalanceAsync(Guid userId, Guid groupId, decimal amountToAdd)
    {
        var userGroup = await context.UserGroups.FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == groupId);
        if (userGroup == null) throw new Exception("User not found in group");
        userGroup.Balance += Math.Round(amountToAdd, 2);
        await context.SaveChangesAsync();
    }

    public async Task ResetBalanceAsync(Guid userId, Guid groupId)
    {
        var userGroup = await context.UserGroups.FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == groupId);
        if (userGroup == null) throw new Exception("User not found in group");
        userGroup.Balance = decimal.Zero;
        await context.SaveChangesAsync();
    }

    public async Task<ICollection<KeyValuePair<Guid, decimal>>> GetGroupBalances(Guid groupId)
    {
        return await context.UserGroups
            .Where(ug => ug.GroupId == groupId)
            .Select(ug => new KeyValuePair<Guid, decimal>(ug.UserId, ug.Balance))
            .ToListAsync();
    }
}