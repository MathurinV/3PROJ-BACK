namespace DAL.Models.Messages;

public class MessageInsertInput
{
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = null!;
    
    public MessageInsertDto ToMessageInsertDto(Guid senderId) => new()
    {
        SenderId = senderId,
        ReceiverId = ReceiverId,
        Content = Content
    };
}