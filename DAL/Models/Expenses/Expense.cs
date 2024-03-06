using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;
using DAL.Models.Groups;
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

    public string JustificationUrl()
    {
        if (Id == Guid.Empty) throw new Exception("Please also include the expense id in your query");

        var hashedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Id.ToString()));
        var timestamp = DateTime.UtcNow.Ticks;
        return $"https://localhost:3000/justification/{hashedId}?t={timestamp}";
    }
}