using DAL.Models.UserExpenses;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class UserExpenseRepository(MoneyMinderDbContext context) : IUserExpenseRepository
{
    public async Task<ICollection<UserExpense>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto)
    {
        var userExpenses = userExpenseInsertDto.Select(x => x.ToUserExpense());
        var enumerable = userExpenses.ToList();
        await context.UserExpenses.AddRangeAsync(enumerable);
        if (await context.SaveChangesAsync() == 0) throw new Exception("Failed to insert user expenses");
        var expense = await context.Expenses
            .Include(x => x.CreatedBy)
            .Include(x => x.UserExpenses)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == enumerable.First().ExpenseId);
        if (expense == null) throw new Exception("Expense not found");
        return expense.UserExpenses.Where(x => x.ExpenseId == expense.Id).ToList();
    }

    public async Task SetPaidAtAsync(Guid groupId, Guid userId)
    {
        var dateTime = DateTime.UtcNow;
        var userExpenses = await context.UserExpenses
            .Include(x => x.Expense)
            .Where(x => x.Expense.GroupId == groupId && x.UserId == userId)
            .ToListAsync();
        foreach (var userExpense in userExpenses)
        {
            userExpense.PaidAt = dateTime;
        }

        await context.SaveChangesAsync();
    }
}