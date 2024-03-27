using System.Security.Claims;
using API.Subscriptions;
using DAL.Models.Messages;
using DAL.Repositories;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class MessageMutations
{
    [Authorize]
    public async Task<Message?> SendMessage([FromServices] IMessageRepository messageRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [Service] ITopicEventSender eventSender,
        MessageInsertInput messageInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        var tmpMessage = await messageRepository.InsertAsync(messageInsertInput.ToMessageInsertDto(Guid.Parse(userId)));
        await eventSender.SendAsync(nameof(MessageSubscriptions.MessageAdded), tmpMessage);
        return tmpMessage;
    }

    [Authorize]
    public async Task<GroupMessage?> SendGroupMessage([FromServices] IGroupMessageRepository groupMessageRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        GroupMessageInsertInput groupMessageInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        return await groupMessageRepository.InsertAsync(
            groupMessageInsertInput.ToGroupMessageInsertDto(Guid.Parse(userId)));
    }
}