using DAL.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace DAL.Repositories;

public interface IUserRepository
{
    /// <summary>
    ///     Inserts a new AppUser into the database asynchronously.
    /// </summary>
    /// <param name="appUserInsertDto">The AppUserInsertDto object containing the user details.</param>
    /// <returns>Returns a Task of AppUser. The task will be completed with the inserted AppUser or null if there was an error.</returns>
    Task<AppUser?> InsertAsync(AppUserInsertDto appUserInsertDto);

    /// <summary>
    ///     Signs in a user asynchronously.
    /// </summary>
    /// <param name="appUserLoginDto">The AppUserLoginDto object containing the user login details.</param>
    /// <returns>Returns a SignInResult object representing the result of the sign-in operation.</returns>
    Task<SignInResult> SignInAsync(AppUserLoginDto appUserLoginDto);

    /// <summary>
    ///     Signs out the current user asynchronously.
    /// </summary>
    /// <returns>Returns a boolean value indicating whether the sign-out operation was successful.</returns>
    Task<bool> SignOutAsync();

    /// <summary>
    ///     Adds the specified amount to the balance of a user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="amount">The amount to be added to the user's balance.</param>
    /// <returns>
    ///     A Task that represents the asynchronous operation. The task will be completed with a boolean value indicating
    ///     whether the operation was successful (true) or not (false).
    /// </returns>
    Task<bool> AddToBalanceAsync(Guid userId, decimal amount);

    /// <summary>
    ///     Adds the specified amounts to the balances of the users asynchronously.
    /// </summary>
    /// <param name="userIdAmountPairs">
    ///     A collection of key-value pairs representing the user IDs and the amounts to be added
    ///     to their balances.
    /// </param>
    /// <returns>
    ///     A Task that represents the asynchronous operation. The task will be completed with a boolean value indicating
    ///     whether the operation was successful (true) or not (false).
    /// </returns>
    Task<bool> AddToBalancesAsync(ICollection<KeyValuePair<Guid, decimal>> userIdAmountPairs);

    /// <summary>
    ///     Deletes a user record from the database asynchronously.
    /// </summary>
    /// <param name="id">The ID of the user to be deleted.</param>
    /// <returns>
    ///     A Task that represents the asynchronous operation. The task will be completed with a boolean value indicating
    ///     whether the user record was successfully deleted (true) or not (false).
    /// </returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Retrieves all users from the database.
    /// </summary>
    /// <returns>Returns an IQueryable of AppUser representing the collection of all users in the database.</returns>
    IQueryable<AppUser> GetAll();

    /// <summary>
    ///     Retrieves an AppUser by its ID.
    /// </summary>
    /// <param name="id">The ID of the AppUser.</param>
    /// <returns>Returns an IQueryable of AppUser that matches the specified ID, or null if no match is found.</returns>
    IQueryable<AppUser?> GetById(Guid id);

    /// <summary>
    ///     Retrieves an AppUser by email from the database asynchronously.
    /// </summary>
    /// <param name="email">The email of the AppUser to retrieve. Can't be null.</param>
    /// <returns>Returns an IQueryable of AppUser that matches the provided email.</returns>
    IQueryable<AppUser?> GetByEmail(string email = null!);

    Task<AppUser?> ModifyAsync(Guid userId, AppUserModifyDto appUserModifyDto);

    Task<AppUser?> ChangeMyPasswordAsync(Guid userId, AppUserModifyDto appUserModifyDto);

    Task<AppUser?> GetByIdAsync(Guid userId);

    Task<bool> ChangeAvatarExtensionAsync(Guid userId,
        ImageFileTypes.ValidImageExtensions? newExtension);

    IQueryable<AppUser> GetFriends(Guid userId);
}