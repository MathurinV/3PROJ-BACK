using DAL.Models.Groups;

namespace DAL.Repositories;

public interface IGroupRepository
{
    public Task<ICollection<Group>> GetAllAsync();
    public Task<Group?> GetByIdAsync(Guid id);
    public Task<Group?> InsertAsync(GroupInsertDto groupInsertDto);
}