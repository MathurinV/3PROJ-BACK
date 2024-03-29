using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Expenses;

public class ExpenseInsertInput
{
    public ICollection<KeyValuePair<Guid, decimal>> UsersWithAmount { get; set; } = null!;
    public Guid GroupId { get; set; }
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