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

    public async Task<ICollection<KeyValuePair<Guid, decimal>>> PayDuesByUserIdAsync(Guid userId)
    {
        var user = await context.Users
            .Include(x => x.UserExpenses)
            .ThenInclude(x => x.Expense)
            .ThenInclude(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) throw new Exception("User not found");
        var userExpenses = user.UserExpenses.Where(x => x.PaidAt == null);
        var enumerable = userExpenses as UserExpense[] ?? userExpenses.ToArray();
        var moneyDue = enumerable.Sum(ue => ue.Amount);
        if (user.Balance < moneyDue) throw new Exception("Insufficient funds");
        List<KeyValuePair<Guid, decimal>> result = new();
        foreach (var userExpense in enumerable)
        {
            var toBePaidValue = userExpense.Amount;
            var expenseCreatorId = userExpense.Expense.CreatedBy.Id;
            result.Add(new KeyValuePair<Guid, decimal>(expenseCreatorId, toBePaidValue));
            userExpense.PaidAt = DateTime.UtcNow;
        }

        user.Balance -= moneyDue;

        await context.SaveChangesAsync();
        return result;
    }

    public async Task<ICollection<KeyValuePair<Guid, decimal>>> PayDuesByUserIdAndExpenseIdsAsync(Guid userId,
        ICollection<Guid> expenseIds)
    {
        var user = await context.Users
            .Include(x => x.UserExpenses)
            .ThenInclude(x => x.Expense)
            .ThenInclude(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) throw new Exception("User not found");
        var userExpenses = user.UserExpenses.Where(x => x.PaidAt == null && expenseIds.Contains(x.ExpenseId));
        var enumerable = userExpenses as UserExpense[] ?? userExpenses.ToArray();
        var moneyDue = enumerable.Sum(ue => ue.Amount);
        if (user.Balance < moneyDue) throw new Exception("Insufficient funds");
        List<KeyValuePair<Guid, decimal>> result = new();
        foreach (var userExpense in enumerable)
        {
            var toBePaidValue = userExpense.Amount;
            var expenseCreatorId = userExpense.Expense.CreatedBy.Id;
            result.Add(new KeyValuePair<Guid, decimal>(expenseCreatorId, toBePaidValue));
            userExpense.PaidAt = DateTime.UtcNow;
        }

        user.Balance -= moneyDue;

        await context.SaveChangesAsync();
        return result;
    }
}