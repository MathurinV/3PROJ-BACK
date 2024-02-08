using DAL.Models.Expenses;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.UserExpenses;

public class UserExpense
{
    [GraphQLIgnore] public Guid ExpenseId { get; set; }
    public Expense Expense { get; set; } = null!;
    [GraphQLIgnore] public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; } = null;
}