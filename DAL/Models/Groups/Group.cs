using System.ComponentModel.DataAnnotations;
using DAL.Models.Expenses;
using DAL.Models.Invitations;
using DAL.Models.Messages;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.Groups;

public class Group
{
    public Guid Id { get; set; }
    [StringLength(255)] public string Name { get; set; } = null!;
    [StringLength(255)] public string Description { get; set; } = null!;
    [StringLength(1000)] public string? Image { get; set; }
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    [GraphQLIgnore] public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public ICollection<GroupMessage> ReceivedGroupMessages { get; set; } = new List<GroupMessage>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}