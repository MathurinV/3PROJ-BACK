using DAL.Models.Messages;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class MessageMutations
{
    public async Task<Message?> SendMessage([FromServices] IMessageRepository messageRepository,
        MessageInsertDto messageInsertDto)
    {
        return await messageRepository.InsertAsync(messageInsertDto);
    }

    public async Task<GroupMessage?> SendGroupMessage([FromServices] IGroupMessageRepository groupMessageRepository,
        GroupMessageInsertDto groupMessageInsertDto)
    {
        return await groupMessageRepository.InsertAsync(groupMessageInsertDto);
    }
}