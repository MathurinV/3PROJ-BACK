using DAL.Models.UserExpenses;
using DAL.Repositories;

namespace API.Repositories;

public class UserExpenseRepository(MoneyMinderDbContext context):IUserExpenseRepository
{
    public async Task<ICollection<UserExpense?>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto)
    {
        var userExpenses = userExpenseInsertDto.Select(x => x.ToUserExpense());
        var enumerable = userExpenses.ToList();
        await context.UserExpenses.AddRangeAsync(enumerable);
        await context.SaveChangesAsync();
        return enumerable.Select(x => (UserExpense?)x).ToList();
    }
}