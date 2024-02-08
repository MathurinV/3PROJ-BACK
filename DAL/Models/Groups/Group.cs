using System.ComponentModel.DataAnnotations;
using DAL.Models.Messages;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.Groups;

public class Group
{
    public Guid Id { get; set; }
    [StringLength(50)] public string Name { get; set; } = null!;
    [StringLength(50)] public string Description { get; set; } = null!;
    [StringLength(50)] public string? Image { get; set; }
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    [GraphQLIgnore]public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public ICollection<GroupMessage> ReceivedGroupMessages { get; set; } = new List<GroupMessage>();
}