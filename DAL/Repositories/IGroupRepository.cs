using DAL.Models.Groups;

namespace DAL.Repositories;

public interface IGroupRepository
{
    IQueryable<Group> GetAll();
    IQueryable<Group?> GetById(Guid id);

    Task<Group?> InsertAsync(GroupInsertDto groupInsertDto);
    Task<Group?> GetByIdAsync(Guid currentGroupId);
}