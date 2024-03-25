using DAL.Models.Messages;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Subscriptions;

[ExtendObjectType("Subscription")]
public class MessageSubscriptions
{
    [Subscribe]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Message?> MessageAdded([EventMessage] Message message,
        [FromServices] IMessageRepository messageRepository)
    {
        return messageRepository.GetMessageById(message.Id);
    }
}