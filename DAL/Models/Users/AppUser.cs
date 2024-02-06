using DAL.Models.Groups;
using DAL.Models.UserGroups;
using Microsoft.AspNetCore.Identity;

namespace DAL.Models.Users;

public class AppUser : IdentityUser<Guid>
{
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
}