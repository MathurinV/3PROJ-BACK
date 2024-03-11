using System.ComponentModel.DataAnnotations;
using DAL.Models.Groups;
using DAL.Models.Justifications;
using DAL.Models.UserExpenses;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.Expenses;

public class Expense
{
    public Guid Id { get; set; }
    [GraphQLIgnore] public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public decimal Amount { get; set; }
    [StringLength(255)] public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    [GraphQLIgnore] public Guid CreatedById { get; set; }
    public AppUser CreatedBy { get; set; } = null!;
    public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();
    [GraphQLIgnore] public Justification? Justification { get; set; } = null!;
}