using DAL.Models.Messages;

namespace DAL.Repositories;

public interface IGroupMessageRepository
{
    Task<GroupMessage?> InsertAsync(GroupMessageInsertDto groupMessageInsertDto);
}