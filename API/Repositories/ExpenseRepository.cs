using DAL.Models.Expenses;
using DAL.Repositories;

namespace API.Repositories;

public class ExpenseRepository(MoneyMinderDbContext context): IExpenseRepository
{
    public async Task<Expense?> InsertAsync(ExpenseInsertDto expenseInsertDto)
    {
        var expense = expenseInsertDto.ToExpense();
        var tmp = await context.Expenses.AddAsync(expense);
        await context.SaveChangesAsync();
        return tmp.Entity;
    }
}