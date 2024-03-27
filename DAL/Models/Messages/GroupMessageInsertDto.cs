using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Messages;

public class GroupMessageInsertDto
{
    public Guid SenderId { get; set; }
    public Guid GroupId { get; set; }
    [StringLength(1000)] public string Content { get; set; } = null!;

    public GroupMessage ToGroupMessage()
    {
        return new GroupMessage
        {
            SenderId = SenderId,
            GroupId = GroupId,
            Content = Content
        };
    }
}