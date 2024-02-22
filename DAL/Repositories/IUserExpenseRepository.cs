using DAL.Models.UserExpenses;

namespace DAL.Repositories;

public interface IUserExpenseRepository
{
    public Task<ICollection<UserExpense?>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto);
    public Task<bool> PayByUserId(Guid userId);
}