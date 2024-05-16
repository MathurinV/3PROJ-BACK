using DAL.Models.UserExpenses;

namespace DAL.Repositories;

public interface IUserExpenseRepository
{
    Task<ICollection<UserExpense>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto);
}