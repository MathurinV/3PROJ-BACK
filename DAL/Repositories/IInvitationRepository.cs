using DAL.Models.Invitations;

namespace DAL.Repositories;

public interface IInvitationRepository
{
    /// <summary>
    /// Inserts a new invitation into the database.
    /// </summary>
    /// <param name="invitationInsertDto">The invitation insert DTO containing the group ID and invited user ID.</param>
    /// <returns>The inserted invitation object or null if insertion fails.</returns>
    Task<Invitation?> InsertAsync(InvitationInsertDto invitationInsertDto);

    /// <summary>
    /// Deletes an invitation from the database. Used when a user accepts or rejects an invitation.
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="userId">The ID of the user</param>
    /// <returns>Returns true if the invitation was deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(Guid groupId, Guid userId);
}