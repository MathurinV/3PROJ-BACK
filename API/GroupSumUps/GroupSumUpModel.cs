using DAL.Models.Expenses;
using DAL.Models.Groups;

namespace API.GroupSumUps;

public class GroupSumUpModel
{
    public string GroupName { get; set; } = null!;
    public ICollection<GroupSumUpExpense> Expenses { get; set; } = null!;
    public DateTime CurrentDate { get; } = DateTime.Now;
    public ICollection<GroupSumUpUser> Payers { get; set; } = new List<GroupSumUpUser>();
}

public class GroupSumUpExpense
{
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public ExpenseType Type { get; set; }
}

public class GroupSumUpUser
{
    public string? UserName { get; set; } = null!;
    public DateTime? PaidAt { get; set; }
    public decimal? AmountDue { get; set; }
    public string? ToUserName { get; set; }
}