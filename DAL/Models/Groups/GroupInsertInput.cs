using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Groups;

public class GroupInsertInput
{
    [StringLength(255)] public string Name { get; set; } = null!;
    [StringLength(255)] public string Description { get; set; } = null!;
    [StringLength(1000)] public string? Image { get; set; }

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