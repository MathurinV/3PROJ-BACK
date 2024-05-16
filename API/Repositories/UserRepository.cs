using DAL;
using DAL.Models.Expenses;
using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class UserRepository(
    MoneyMinderDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    SignInManager<AppUser> signInManager) : IUserRepository
{
    public async Task<AppUser?> InsertAsync(AppUserInsertDto appUserInsertDto)
    {
        var appUser = appUserInsertDto.ToAppUser();

        var result = await userManager.CreateAsync(appUser, appUserInsertDto.Password);
        if (!result.Succeeded) throw new Exception("Error creating user");
        if (!await roleManager.RoleExistsAsync(appUserInsertDto.Role)) throw new Exception("Role not found");
        await userManager.AddToRoleAsync(appUser, appUserInsertDto.Role);
        return appUser;
    }

    public async Task<SignInResult> SignInAsync(AppUserLoginDto appUserLoginDto)
    {
        return await signInManager.PasswordSignInAsync(appUserLoginDto.Username, appUserLoginDto.Password,
            appUserLoginDto.RememberMe, false);
    }

    public async Task<bool> SignOutAsync()
    {
        await signInManager.SignOutAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) throw new Exception("User not found");
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }

    public IQueryable<AppUser> GetAll()
    {
        return context.Users;
    }

    public IQueryable<AppUser?> GetById(Guid id)
    {
        return context.Users.Where(u => u.Id == id);
    }

    public async Task<AppUser?> ModifyAsync(Guid userId, AppUserModifyDto appUserModifyDto)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");

        if (appUserModifyDto.Email != null) user.Email = appUserModifyDto.Email;
        if (appUserModifyDto.UserName != null) user.UserName = appUserModifyDto.UserName;
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<AppUser?> ChangeMyPasswordAsync(Guid userId, AppUserModifyDto appUserModifyDto)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");
        if (appUserModifyDto.Password != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            context.Entry(user).State = EntityState.Detached;
            user = await context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");
            var result = await userManager.ResetPasswordAsync(user, token, appUserModifyDto.Password);
            if (!result.Succeeded) throw new Exception("Error changing password");
        }

        return user;
    }

    public IQueryable<AppUser?> GetByEmail(string email = null!)
    {
        return context.Users.Where(u => u.Email == email);
    }

    public async Task<AppUser?> GetByIdAsync(Guid userId)
    {
        return await context.Users.FindAsync(userId);
    }

    public async Task<bool> ChangeAvatarExtensionAsync(Guid userId, ImageFileTypes.ValidImageExtensions? newExtension)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");
        user.AvatarExtension = newExtension;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task ChangeRibExtensionAsync(Guid entityId,
        JustificationFileTypes.ValidJustificationExtensions? newExtension)
    {
        var user = await context.Users.FindAsync(entityId);
        if (user == null) throw new Exception("User not found");
        user.RibExtension = newExtension;
        await context.SaveChangesAsync();
    }

    public IQueryable<AppUser> GetFriends(Guid userId)
    {
        var friends = context.Users.Where(au => au.SentMessages.Any(sm => sm.SenderId == userId) ||
                                                au.ReceivedMessages.Any(rm => rm.ReceiverId == userId));
        return friends;
    }

    public async Task<bool> IsDueToAsync(Guid currentUserId, Guid otherUserId)
    {
        var user = await context.Users.FindAsync(currentUserId);
        if (user == null) throw new Exception("User not found");
        var otherUser = await context.Users.FindAsync(otherUserId);
        if (otherUser == null) throw new Exception("User not found");
        var currentUserGroups = context.UserGroups.Where(ug => ug.UserId == currentUserId);
        return currentUserGroups.Any(ug => ug.PayTo.PayToUserId == otherUserId);
    }
}