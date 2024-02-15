namespace DAL.Models.Invitations;

public class InvitationInsertDto
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }

    public Invitation ToInvitation()
    {
        return new Invitation
        {
            GroupId = GroupId,
            UserId = UserId
        };
    }
}