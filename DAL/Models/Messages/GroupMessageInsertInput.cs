using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Messages;

public class GroupMessageInsertInput
{
    public Guid GroupId { get; set; }
    [StringLength(1000)] public string Content { get; set; } = null!;

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