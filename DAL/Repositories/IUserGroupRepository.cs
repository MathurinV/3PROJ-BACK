using DAL.Models.UserGroups;

namespace DAL.Repositories;

public interface IUserGroupRepository
{
    Task<UserGroup?> InsertAsync(UserGroupInsertDto userGroupInsertDto);
    Task<bool> IsUserInGroup(Guid userId, Guid groupId);
    Task<bool> AreUsersInGroup(Guid groupId, IEnumerable<Guid> userIds);
}