using DAL.Models.Groups;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.Invitations;

public class Invitation
{
    [GraphQLIgnore] public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    [GraphQLIgnore] public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public DateTime InvitedAt { get; set; }
}