using DAL.Models.Expenses;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace API.Repositories;

public class ExpenseRepository(MoneyMinderDbContext context) : IExpenseRepository
{
    public async Task<Expense?> InsertAsync(ExpenseInsertDto expenseInsertDto)
    {
        var expense = expenseInsertDto.ToExpense();
        var tmp = await context.Expenses.AddAsync(expense);
        await context.SaveChangesAsync();
        return tmp.Entity;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return context.Database.BeginTransactionAsync();
    }

    public async Task<Expense?> GetByIdAsync(Guid id)
    {
        return await context.Expenses.FindAsync(id);
    }
}