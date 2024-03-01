using DAL.Models.Messages;
using DAL.Repositories;

namespace API.Repositories;

public class GroupMessageRepository(MoneyMinderDbContext context) : IGroupMessageRepository
{
    public async Task<GroupMessage?> InsertAsync(GroupMessageInsertDto groupMessageInsertDto)
    {
        var groupMessage = groupMessageInsertDto.ToGroupMessage();
        var tmp = await context.GroupMessages.AddAsync(groupMessage);
        await context.SaveChangesAsync();
        return tmp.Entity;
    }
}