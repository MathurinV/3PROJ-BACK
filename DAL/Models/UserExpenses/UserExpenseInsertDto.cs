namespace DAL.Models.UserExpenses;

public class UserExpenseInsertDto
{
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }

    public UserExpense ToUserExpense()
    {
        return new UserExpense
        {
            ExpenseId = ExpenseId,
            UserId = UserId,
            Amount = Amount
        };
    }
}