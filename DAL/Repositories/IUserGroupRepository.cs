using DAL.Models.UserGroups;

namespace DAL.Repositories;

public interface IUserGroupRepository
{
    Task<UserGroup?> InsertAsync(UserGroupInsertDto userGroupInsertDto);
}