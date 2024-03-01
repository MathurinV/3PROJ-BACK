using DAL.Models.Groups;

namespace DAL.Repositories;

public interface IGroupRepository
{
    IQueryable<Group> GetAll();
    Task<Group?> GetByIdAsync(Guid currentGroupId);
    IQueryable<Group?> GetById(Guid id);
    Task<Group?> InsertAsync(GroupInsertDto groupInsertDto);
}