using DAL.Models.Groups;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.UserGroups;

/// <summary>
/// Represents a relationship between a user and a group.
/// </summary>
public class UserGroup
{
    /// <summary>
    /// Represents the unique identifier of a user.
    /// </summary>
    /// <remarks>
    /// This property is ignored in GraphQL queries.
    /// </remarks>
    [GraphQLIgnore]
    public Guid UserId { get; set; }

    /// <summary>
    /// Represents a user.
    /// </summary>
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Represents the unique identifier of a user group.
    /// </summary>
    /// <remarks>
    /// This property is ignored in GraphQL queries.
    /// </remarks>
    [GraphQLIgnore]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Represents a group.
    /// </summary>
    public Group Group { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when a user joined the user group.
    /// </summary>
    public DateTime JoinedAt { get; set; }
}