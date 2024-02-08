using DAL.Models.Messages;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class MessageRepository(MoneyMinderDbContext context) : IMessageRepository
{
    public async Task<ICollection<Message>> GetAllAsync()
    {
        return await context.Messages.ToListAsync();
    }

    public async Task<Message?> InsertAsync(MessageInsertDto messageInsertDto)
    {
        var message = messageInsertDto.ToMessage();
        var tmp = await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();
        return tmp.Entity;
    }
}