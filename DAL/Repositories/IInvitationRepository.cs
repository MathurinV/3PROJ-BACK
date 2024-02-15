using DAL.Models.Invitations;

namespace DAL.Repositories;

public interface IInvitationRepository
{
    Task<Invitation?> InsertAsync(InvitationInsertDto invitationInsertDto);
    Task<bool> DeleteAsync(Guid groupId, Guid userId);
}