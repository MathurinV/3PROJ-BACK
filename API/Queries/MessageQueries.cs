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
    public IQueryable<Message> GetMessagesByOtherUserId([FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IMessageRepository messageRepository, [FromServices] IUserRepository userRepository,
        Guid otherUserId)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) throw new Exception("Issue with getting user id");
        var userId = Guid.Parse(userIdString);
        var currentUser = userRepository.GetByIdAsync(userId);
        var otherUser = userRepository.GetByIdAsync(otherUserId);
        if (currentUser.Result == null || otherUser.Result == null) throw new Exception("User not found");
        return messageRepository.GetMessagesByOtherUserId(userId, otherUserId);
    }
}