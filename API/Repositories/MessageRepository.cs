using DAL.Models.Messages;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class MessageRepository(MoneyMinderDbContext context) : IMessageRepository
{
    public async Task<Message?> InsertAsync(MessageInsertDto messageInsertDto)
    {
        var message = messageInsertDto.ToMessage();
        var tmp = await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();
        return tmp.Entity;
    }

    public IQueryable<Message?> GetMessageById(Guid messageId)
    {
        return context.Messages.Where(x => x.Id == messageId);
    }

    public Task<List<Message>> GetMessagesByOtherUserId(Guid currentUserId, Guid otherUserId)
    {
        return context.Messages.Where(x => x.SenderId == currentUserId && x.ReceiverId == otherUserId ||
                                           x.SenderId == otherUserId && x.ReceiverId == currentUserId).ToListAsync();
    }
}