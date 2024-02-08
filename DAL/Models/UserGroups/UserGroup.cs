using DAL.Models.Groups;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.UserGroups;

public class UserGroup
{
    [GraphQLIgnore] public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    [GraphQLIgnore] public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}