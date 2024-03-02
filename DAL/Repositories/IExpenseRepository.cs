using System.Data;
using DAL.Models.Expenses;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories;

public interface IExpenseRepository
{
    Task<Expense?> InsertAsync(ExpenseInsertDto expenseInsertDto);
    Task<IDbContextTransaction> BeginTransactionAsync();
}