using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.Messages;

public class Message
{
    public Guid Id { get; set; }
    [GraphQLIgnore]public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;
    [GraphQLIgnore]public Guid ReceiverId { get; set; }
    public AppUser Receiver { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
}