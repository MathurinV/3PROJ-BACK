using DAL.Models.Expenses;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories;

public interface IExpenseRepository
{
    /// <summary>
    ///     Inserts a new expense into the database.
    /// </summary>
    /// <param name="expenseInsertDto">The DTO object containing the details of the expense to be inserted.</param>
    /// <returns>Returns a Task object representing the asynchronous operation. The task result is the inserted expense object.</returns>
    Task<Expense?> InsertAsync(ExpenseInsertDto expenseInsertDto);

    /// <summary>
    ///     Begins a new database transaction.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result is an IDbContextTransaction representing the
    ///     started transaction.
    /// </returns>
    Task<IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    ///     Gets the expense with the specified ID from the database asynchronously.
    /// </summary>
    /// <param name="id">The ID of the expense to get.</param>
    /// <returns>The task object representing the asynchronous operation. The task result is the expense with the specified ID.</returns>
    Task<Expense?> GetByIdAsync(Guid id);
}