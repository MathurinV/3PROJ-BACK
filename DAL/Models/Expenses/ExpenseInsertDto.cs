namespace DAL.Models.Expenses;

public class ExpenseInsertDto
{
    public ICollection<KeyValuePair<Guid, decimal>> WeightedUsers { get; set; } = null!;
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public Guid CreatedById { get; set; }

    public Expense ToExpense()
    {
        return new Expense
        {
            GroupId = GroupId,
            Amount = Amount,
            Description = Description,
            CreatedById = CreatedById
        };
    }
}