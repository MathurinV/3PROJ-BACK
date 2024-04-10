using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Expenses;

public class ExpenseInsertInput
{
    public ICollection<KeyValuePair<Guid, decimal?>> UserAmountsList { get; set; } = null!;
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    [StringLength(255)] public string Description { get; set; } = null!;

    public ExpenseInsertDto ToExpenseInsertDto(Guid createdById)
    {
        return new ExpenseInsertDto
        {
            GroupId = GroupId,
            Description = Description,
            CreatedById = createdById
        };
    }
}