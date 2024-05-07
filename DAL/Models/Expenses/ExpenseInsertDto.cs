using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Expenses;

public class ExpenseInsertDto
{
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    [StringLength(255)] public string Description { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public ExpenseType ExpenseType { get; set; }

    public Expense ToExpense()
    {
        return new Expense
        {
            GroupId = GroupId,
            Amount = Amount,
            Description = Description,
            CreatedById = CreatedById,
            ExpenseType = ExpenseType
        };
    }
}