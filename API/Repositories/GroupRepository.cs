using DAL.Models.Groups;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class GroupRepository(MoneyMinderDbContext context) : IGroupRepository
{
    public async Task<ICollection<Group>> GetAllAsync()
    {
        return await context.Groups
            .Include(g => g.UserGroups)
            .ThenInclude(ug => ug.User)
            .Include(g => g.Owner)
            .ToListAsync();
    }

    public async Task<Group?> GetByIdAsync(Guid id)
    {
        return await context.Groups
            .Include(g => g.UserGroups)
            .ThenInclude(ug => ug.User)
            .Include(g => g.Owner)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Group?> InsertAsync(GroupInsertDto groupInsertDto)
    {
        var group = groupInsertDto.ToGroup();
        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync();
        return group;
    }
}