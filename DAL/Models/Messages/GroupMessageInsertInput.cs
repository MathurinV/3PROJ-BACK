namespace DAL.Models.Messages;

public class GroupMessageInsertInput
{
    public Guid GroupId { get; set; }
    public string Content { get; set; } = null!;

    public GroupMessageInsertDto ToGroupMessageInsertDto(Guid senderId)
    {
        return new GroupMessageInsertDto
        {
            SenderId = senderId,
            GroupId = GroupId,
            Content = Content
        };
    }
}