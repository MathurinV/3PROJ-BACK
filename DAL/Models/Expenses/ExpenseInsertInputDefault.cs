using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Expenses;

public class ExpenseInsertInputDefault
{
    public ICollection<Guid> UsersIds { get; set; } = null!;
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    [StringLength(255)] public string Description { get; set; } = null!;

    public ExpenseInsertDto ToExpenseInsertDto(Guid createdById)
    {
        return new ExpenseInsertDto
        {
            WeightedUsers = UsersIds.Select(x => new KeyValuePair<Guid, decimal>(x, 1)).ToList(),
            GroupId = GroupId,
            Amount = Amount,
            Description = Description,
            CreatedById = createdById
        };
    }
}