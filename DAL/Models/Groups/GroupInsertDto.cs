using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Groups;

public class GroupInsertDto
{
    [StringLength(255)] public string Name { get; set; } = null!;
    [StringLength(255)] public string Description { get; set; } = null!;
    public Guid OwnerId { get; set; }

    public Group ToGroup()
    {
        return new Group
        {
            Name = Name,
            Description = Description,
            OwnerId = OwnerId
        };
    }
}