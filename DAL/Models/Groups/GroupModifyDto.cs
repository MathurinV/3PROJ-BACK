using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Groups;

public class GroupModifyDto
{
    public Guid GroupId { get; set; }
    [StringLength(255)] public string? Name { get; set; }
    [StringLength(255)] public string? Description { get; set; }
    public Guid? OwnerId { get; set; }
}