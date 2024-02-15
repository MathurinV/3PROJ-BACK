using DAL.Models.Groups;
using DAL.Models.Users;

namespace DAL.Models.Invitations;

public class Invitation
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public DateTime InvitedAt { get; set; }
}