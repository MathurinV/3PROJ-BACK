using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.Messages;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using Microsoft.AspNetCore.Identity;

namespace DAL.Models.Users;

public class AppUser : IdentityUser<Guid>
{
    public decimal Balance { get; set; } = 0;
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<GroupMessage> SentGroupMessages { get; set; } = new List<GroupMessage>();
    public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();
    public ICollection<Expense> CreatedExpenses { get; set; } = new List<Expense>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}