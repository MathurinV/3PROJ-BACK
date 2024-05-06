using DAL.Models.Groups;

namespace DAL.Repositories;

public interface IGroupRepository
{
    /// <summary>
    ///     Returns all groups.
    /// </summary>
    /// <returns>The collection of all groups.</returns>
    IQueryable<Group> GetAll();

    /// <summary>
    ///     Get a group by its ID asynchronously.
    /// </summary>
    /// <param name="currentGroupId">The ID of the group to retrieve.</param>
    /// <returns>The group with the specified ID, or null if not found.</returns>
    Task<Group?> GetByIdAsync(Guid currentGroupId);

    /// <summary>
    ///     Get a group by its ID synchronously.
    /// </summary>
    /// <param name="id">The ID of the group to retrieve.</param>
    /// <returns>The group with the specified ID, or null if not found.</returns>
    IQueryable<Group?> GetById(Guid id);

    /// <summary>
    ///     Inserts a new group into the database.
    /// </summary>
    /// <param name="groupInsertDto">The GroupInsertDto object containing the group information to insert.</param>
    /// <returns>The inserted group object if successful, or null if an error occurred.</returns>
    Task<Group?> InsertAsync(GroupInsertDto groupInsertDto);
    
    Task<Group?> ModifyAsync(Guid userModifierId, GroupModifyDto groupModifyDto);

    public Task<bool> ChangeGroupImageExtensionAsync(Guid groupId, ImageFileTypes.ValidImageExtensions? newExtension);
}