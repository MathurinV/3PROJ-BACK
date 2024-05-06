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

    public async Task<Group?> ModifyAsync(Guid userModifierId, GroupModifyDto groupModifyDto)
    {
        var group = await context.Groups.FindAsync(groupModifyDto.GroupId);
        if (group == null) throw new Exception("Group not found");
        if (group.OwnerId != userModifierId) throw new Exception("User is not the owner of the group");
        if (groupModifyDto.Name != null) group.Name = groupModifyDto.Name;
        if (groupModifyDto.Description != null) group.Description = groupModifyDto.Description;
        if (groupModifyDto.OwnerId != null)
        {
            var newOwner = await context.Users.FindAsync(groupModifyDto.OwnerId);
            if (newOwner == null) throw new Exception("New owner not found");
            var isNewOwnerInGroup = await context.UserGroups.AnyAsync(ug =>
                ug.GroupId == groupModifyDto.GroupId && ug.UserId == groupModifyDto.OwnerId);
            if (isNewOwnerInGroup == false) throw new Exception("New owner is not in the group");
            group.OwnerId = groupModifyDto.OwnerId.Value;
        }
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