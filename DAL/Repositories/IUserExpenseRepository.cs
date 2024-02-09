using DAL.Models.UserExpenses;

namespace DAL.Repositories;

public interface IUserExpenseRepository
{
    public Task<ICollection<UserExpense?>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto);
}