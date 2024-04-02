using System.Security.Claims;
using DAL.Models.Messages;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Queries;

[ExtendObjectType("Query")]
public class MessageQueries
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Message> GetMessagesByOtherUserId([FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IMessageRepository messageRepository, [FromServices] IUserRepository userRepository,
        Guid otherUserId)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           throw new Exception("User not found");
        var userId = Guid.Parse(userIdString);
        return messageRepository.GetMessagesByOtherUserId(userId, otherUserId);
    }
}