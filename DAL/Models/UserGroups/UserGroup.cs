using DAL.Models.Groups;
using DAL.Models.Users;

namespace DAL.Models.UserGroups;

public class UserGroup
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}