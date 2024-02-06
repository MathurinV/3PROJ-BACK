using DAL.Models.UserGroups;
using DAL.Repositories;

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
}