using DAL.Models.Messages;

namespace DAL.Repositories;

public interface IMessageRepository
{
    Task<ICollection<Message>> GetAllAsync();
    Task<Message?> InsertAsync(MessageInsertDto messageInsertDto);
}