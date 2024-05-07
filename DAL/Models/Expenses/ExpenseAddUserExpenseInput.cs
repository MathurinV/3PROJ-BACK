using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Expenses;

public class ExpenseAddUserExpenseInput
{
    public Guid GroupId { get; set; }
    [StringLength(255)] public string Description { get; set; } = null!;
    public Guid CreatedById { get; set; }
    
    public ExpenseType ExpenseType { get; set; }

    public ExpenseInsertDto ToExpenseInsertDto(decimal amount)
    {
        return new ExpenseInsertDto
        {
            Amount = amount,
            Description = Description,
            CreatedById = CreatedById,
            GroupId = GroupId
        };
    }
}