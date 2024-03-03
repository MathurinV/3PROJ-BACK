using DAL.Models.UserGroups;

namespace DAL.Repositories;

public interface IUserGroupRepository
{
    /// <summary>
    /// Inserts a new UserGroup asynchronously.
    /// </summary>
    /// <param name="userGroupInsertDto">The UserGroupInsertDto object containing the UserId and GroupId.</param>
    /// <returns>Returns a Task of UserGroup if the insertion is successful; otherwise, returns null.</returns>
    Task<UserGroup?> InsertAsync(UserGroupInsertDto userGroupInsertDto);

    /// <summary>
    /// Checks if a user is in a group.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="groupId">The ID of the group.</param>
    /// <returns>Returns true if the user is in the group; otherwise, returns false.</returns>
    /// <exception cref="Exception">Thrown when the user is not found in the group.</exception>
    Task<bool> IsUserInGroup(Guid userId, Guid groupId);

    /// <summary>
    /// Checks if given users are in the specified user group.
    /// </summary>
    /// <param name="groupId">The ID of the user group.</param>
    /// <param name="userIds">A collection of user IDs to check.</param>
    /// <returns>True if all users are in the group, false otherwise.</returns>
    Task<bool> AreUsersInGroup(Guid groupId, IEnumerable<Guid> userIds);
}