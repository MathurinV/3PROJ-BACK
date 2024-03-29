using DAL;
using DAL.Models.Groups;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class GroupRepository(MoneyMinderDbContext context) : IGroupRepository
{
    public IQueryable<Group> GetAll()
    {
        return context.Groups;
    }

    public IQueryable<Group?> GetById(Guid id)
    {
        return context.Groups.Where(g => g.Id == id);
    }

    public async Task<Group?> InsertAsync(GroupInsertDto groupInsertDto)
    {
        var group = groupInsertDto.ToGroup();
        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync();
        return group;
    }

    public async Task<bool> ChangeGroupImageExtensionAsync(Guid groupId,
        ImageFileTypes.ValidImageExtensions? newExtension)
    {
        var group = await context.Groups.FindAsync(groupId);
        if (group == null) throw new Exception("User not found");
        group.ImageExtension = newExtension;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Group?> GetByIdAsync(Guid currentGroupId)
    {
        return await context.Groups.FirstOrDefaultAsync(g => g.Id == currentGroupId);
    }
}