using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Groups;

public class GroupInsertInput
{
    [StringLength(50)] public string Name { get; set; } = null!;
    [StringLength(50)] public string Description { get; set; } = null!;
    [StringLength(50)] public string? Image { get; set; }

    public GroupInsertDto ToGroupInsertDto(Guid ownerId)
    {
        return new GroupInsertDto
        {
            Name = Name,
            Description = Description,
            Image = Image,
            OwnerId = ownerId
        };
    }
}