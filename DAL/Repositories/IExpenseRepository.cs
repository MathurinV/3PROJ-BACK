using DAL.Models.Expenses;

namespace DAL.Repositories;

public interface IExpenseRepository
{
    Task<Expense?> InsertAsync(ExpenseInsertDto expenseInsertDto);
}