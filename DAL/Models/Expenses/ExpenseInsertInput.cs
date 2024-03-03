namespace DAL.Models.Expenses;

public class ExpenseInsertInput
{
    public ICollection<KeyValuePair<Guid, decimal>> WeightedUsers { get; set; } = null!;
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;

    public ExpenseInsertDto ToExpenseInsertDto(Guid createdById)
    {
        return new ExpenseInsertDto
        {
            WeightedUsers = WeightedUsers,
            GroupId = GroupId,
            Amount = Amount,
            Description = Description,
            CreatedById = createdById
        };
    }
}