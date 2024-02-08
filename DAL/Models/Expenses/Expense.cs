using DAL.Models.Groups;
using DAL.Models.UserExpenses;
using HotChocolate;

namespace DAL.Models.Expenses;

public class Expense
{
    public Guid Id { get; set; }
    [GraphQLIgnore] public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();
}