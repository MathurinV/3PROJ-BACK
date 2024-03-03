namespace DAL.Models.UserGroups;

/// <summary>
///     Represents the input for joining a user to a group.
/// </summary>
public class UserGroupInsertInput
{
    /// <summary>
    ///     Represents the group id.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    ///     Converts a UserGroupInsertInput object to a UserGroupInsertDto object.
    /// </summary>
    /// <param name="userId">The ID of the user to join the group.</param>
    /// <returns>A UserGroupInsertDto object with the specified user ID and group ID.</returns>
    public UserGroupInsertDto ToUserGroupInsertDto(Guid userId)
    {
        return new UserGroupInsertDto
        {
            UserId = userId,
            GroupId = GroupId
        };
    }
}