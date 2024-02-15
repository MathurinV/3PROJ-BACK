using DAL.Models.Messages;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class MessageMutations
{
    [Authorize]
    public async Task<Message?> SendMessage([FromServices] IMessageRepository messageRepository,
        MessageInsertDto messageInsertDto)
    {
        return await messageRepository.InsertAsync(messageInsertDto);
    }

    [Authorize]
    public async Task<GroupMessage?> SendGroupMessage([FromServices] IGroupMessageRepository groupMessageRepository,
        GroupMessageInsertDto groupMessageInsertDto)
    {
        return await groupMessageRepository.InsertAsync(groupMessageInsertDto);
    }
}