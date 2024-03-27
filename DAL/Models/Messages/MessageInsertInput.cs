using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Messages;

public class MessageInsertInput
{
    public Guid ReceiverId { get; set; }
    [StringLength(1000)] public string Content { get; set; } = null!;

    public MessageInsertDto ToMessageInsertDto(Guid senderId)
    {
        return new MessageInsertDto
        {
            SenderId = senderId,
            ReceiverId = ReceiverId,
            Content = Content
        };
    }
}