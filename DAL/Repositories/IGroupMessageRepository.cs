using DAL.Models.Messages;

namespace DAL.Repositories;

public interface IGroupMessageRepository
{
    public Task<ICollection<GroupMessage>> GetAllAsync();
    public Task<GroupMessage?> InsertAsync(GroupMessageInsertDto groupMessageInsertDto);
}