using DAL.Models.Messages;

namespace DAL.Repositories;

public interface IMessageRepository
{
    public Task<ICollection<Message>> GetAllAsync();
    public Task<Message?> InsertAsync(MessageInsertDto messageInsertDto);
}