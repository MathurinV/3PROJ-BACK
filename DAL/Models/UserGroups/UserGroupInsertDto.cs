namespace DAL.Models.UserGroups;

/// <summary>
/// Represents the data transfer object for inserting a UserGroup.
/// </summary>
public class UserGroupInsertDto
{
    /// <summary>
    /// Represents the User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Represents the GroupId property.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Converts a UserGroupInsertDto to a UserGroup object.
    /// </summary>
    /// <returns>The converted UserGroup object.</returns>
    public UserGroup ToUserGroup()
    {
        return new UserGroup
        {
            UserId = UserId,
            GroupId = GroupId
        };
    }
}