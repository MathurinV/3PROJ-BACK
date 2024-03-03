using DAL.Models.Messages;

namespace DAL.Repositories;

public interface IGroupMessageRepository
{
    /// <summary>
    ///     Inserts a group message into the database.
    /// </summary>
    /// <param name="groupMessageInsertDto">The group message to insert.</param>
    /// <returns>The inserted group message.</returns>
    Task<GroupMessage?> InsertAsync(GroupMessageInsertDto groupMessageInsertDto);
}