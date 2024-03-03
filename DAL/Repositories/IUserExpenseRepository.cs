using DAL.Models.UserExpenses;

namespace DAL.Repositories;

public interface IUserExpenseRepository
{
    /// <summary>
    /// Inserts multiple user expenses into the database.
    /// </summary>
    /// <param name="userExpenseInsertDto">The collection of UserExpenseInsertDto objects representing the user expenses to be inserted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of inserted UserExpense objects.</returns>
    Task<ICollection<UserExpense?>> InsertManyAsync(ICollection<UserExpenseInsertDto> userExpenseInsertDto);

    /// <summary>
    /// Tries to pay all the expenses of a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>Returns a collection of key-value pairs representing the expense creator Ids and the amounts paid by the user.</returns>
    Task<ICollection<KeyValuePair<Guid, decimal>>> PayDuesByUserIdAsync(Guid userId);
}