using DAL.Models.Messages;

namespace API.Subscriptions;

[ExtendObjectType("Subscription")]
public class MessageSubscriptions
{
    [UseFirstOrDefault]
    [UseProjection]
    [Subscribe]
    public Message MessageAdded([EventMessage] Message message) => message;
}