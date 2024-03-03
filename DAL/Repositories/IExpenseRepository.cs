using DAL.Models.Expenses;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories;

public interface IExpenseRepository
{
    /// <summary>
    /// Inserts a new expense into the database.
    /// </summary>
    /// <param name="expenseInsertDto">The DTO object containing the details of the expense to be inserted.</param>
    /// <returns>Returns a Task object representing the asynchronous operation. The task result is the inserted expense object.</returns>
    Task<Expense?> InsertAsync(ExpenseInsertDto expenseInsertDto);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result is an IDbContextTransaction representing the started transaction.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync();
}