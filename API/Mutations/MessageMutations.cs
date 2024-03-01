using System.Security.Claims;
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
        [FromServices] IHttpContextAccessor httpContextAccessor,
        MessageInsertInput messageInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        return await messageRepository.InsertAsync(messageInsertInput.ToMessageInsertDto(Guid.Parse(userId)));
    }

    [Authorize]
    public async Task<GroupMessage?> SendGroupMessage([FromServices] IGroupMessageRepository groupMessageRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        GroupMessageInsertInput groupMessageInsertInput)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        return await groupMessageRepository.InsertAsync(groupMessageInsertInput.ToGroupMessageInsertDto(Guid.Parse(userId)));
    }
}