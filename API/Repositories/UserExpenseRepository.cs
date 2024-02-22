using DAL.Models.UserExpenses;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class UserExpenseRepository(MoneyMinderDbContext context) : IUserExpenseRepository
{
    public async Task<ICollection<UserExpense?>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto)
    {
        var userExpenses = userExpenseInsertDto.Select(x => x.ToUserExpense());
        var enumerable = userExpenses.ToList();
        await context.UserExpenses.AddRangeAsync(enumerable);
        await context.SaveChangesAsync();
        return enumerable.Select(x => (UserExpense?)x).ToList();
    }

    public async Task<bool> PayByUserId(Guid userId)
    {
        var user = await context.Users
            .Include(x => x.UserExpenses)
            .ThenInclude(x => x.Expense)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) return false;
        var userExpenses = user.UserExpenses.Where(x => x.PaidAt == null);
        var enumerable = userExpenses as UserExpense[] ?? userExpenses.ToArray();
        var moneyDue = enumerable.Sum(ue => ue.Amount);
        if (user.Balance < moneyDue) return false;
        foreach (var userExpense in enumerable)
        {
            userExpense.PaidAt = DateTime.UtcNow;
        }
        user.Balance -= moneyDue;
        await context.SaveChangesAsync();
        return true;
    }
}