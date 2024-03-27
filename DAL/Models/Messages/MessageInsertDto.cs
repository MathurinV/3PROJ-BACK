using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Messages;

public class MessageInsertDto
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    [StringLength(1000)] public string Content { get; set; } = null!;

    public Message ToMessage()
    {
        return new Message
        {
            SenderId = SenderId,
            ReceiverId = ReceiverId,
            Content = Content
        };
    }
}