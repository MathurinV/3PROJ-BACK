using DAL.Models.Invitations;
using DAL.Repositories;

namespace API.Repositories;

public class InvitationRepository(MoneyMinderDbContext context) : IInvitationRepository
{
    public async Task<Invitation?> InsertAsync(InvitationInsertDto invitationInsertDto)
    {
        var invitation = invitationInsertDto.ToInvitation();
        await context.Invitations.AddAsync(invitation);
        await context.SaveChangesAsync();
        return invitation;
    }

    public async Task<bool> DeleteAsync(Guid groupId, Guid userId)
    {
        var invitation = await context.Invitations.FindAsync(userId, groupId);
        if (invitation == null) return false;
        context.Invitations.Remove(invitation);
        await context.SaveChangesAsync();
        return true;
    }
}