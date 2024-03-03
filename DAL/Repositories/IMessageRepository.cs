using DAL.Models.Messages;

namespace DAL.Repositories;

public interface IMessageRepository
{
    /// <summary>
    /// Inserts a new message asynchronously.
    /// </summary>
    /// <param name="messageInsertDto">The message insert DTO containing the information of the message to be inserted.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the inserted message if successful; otherwise, null.</returns>
    Task<Message?> InsertAsync(MessageInsertDto messageInsertDto);
}