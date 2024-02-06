using DAL.Models.UserGroups;

namespace DAL.Repositories;

public interface IUserGroupRepository
{
    public Task<UserGroup?> InsertAsync(UserGroupInsertDto userGroupInsertDto);
}