using DAL.Models.UserExpenses;

namespace DAL.Repositories;

public interface IUserExpenseRepository
{
    Task<ICollection<UserExpense?>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto);

    /// <summary>
    /// Tries to pay all the expenses of a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>Returns a collection of key-value pairs representing the expense creator Ids and the amounts paid by the user.</returns>
    Task<ICollection<KeyValuePair<Guid, decimal>>> PayDuesByUserIdAsync(Guid userId);
}