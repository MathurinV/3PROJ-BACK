using System.ComponentModel.DataAnnotations.Schema;
using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.Messages;
using DAL.Models.PaymentDetails;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using HotChocolate;
using Microsoft.AspNetCore.Identity;

namespace DAL.Models.Users;

/// <summary>
///     Represents an application user. Inherits from <see cref="IdentityUser" />.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<GroupMessage> SentGroupMessages { get; set; } = new List<GroupMessage>();
    public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();
    public ICollection<Expense> CreatedExpenses { get; set; } = new List<Expense>();

    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<PayDueTo> PaymentsToBeReceived { get; set; } = new List<PayDueTo>();

    [NotMapped]
    public string? AvatarUrl => AvatarExtension != null
        ? $"http://localhost:3000/avatars/{Id}{ImageFileTypes.ValidImageExtensionToString(AvatarExtension)}"
        : null;

    [GraphQLIgnore] public ImageFileTypes.ValidImageExtensions? AvatarExtension { get; set; }
    [GraphQLIgnore] public JustificationFileTypes.ValidJustificationExtensions? RibExtension { get; set; }
}