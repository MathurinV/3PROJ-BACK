using System.Collections;
using DAL.Models.Groups;
using DAL.Models.Messages;
using DAL.Models.UserGroups;
using Microsoft.AspNetCore.Identity;

namespace DAL.Models.Users;

public class AppUser : IdentityUser<Guid>
{
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<GroupMessage> SentGroupMessages { get; set; } = new List<GroupMessage>();
}