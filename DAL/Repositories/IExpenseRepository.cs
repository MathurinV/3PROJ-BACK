using DAL.Models.Expenses;

namespace DAL.Repositories;

public interface IExpenseRepository
{
    public Task<Expense?> InsertAsync(ExpenseInsertDto expenseInsertDto);
}