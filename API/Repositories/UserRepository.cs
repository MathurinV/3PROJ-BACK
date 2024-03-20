using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;

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

    public async Task<bool> AddToBalanceAsync(Guid userId, decimal amount)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");
        user.Balance += amount;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddToBalancesAsync(ICollection<KeyValuePair<Guid, decimal>> userIdAmountPairs)
    {
        foreach (var (userId, amount) in userIdAmountPairs)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");
            user.Balance += amount;
        }

        await context.SaveChangesAsync();
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

    public IQueryable<AppUser?> GetByEmail(string email = null!)
    {
        return context.Users.Where(u => u.Email == email);
    }

    public async Task<AppUser?> GetByIdAsync(Guid userId)
    {
        return await context.Users.FindAsync(userId);
    }

    public async Task<bool> ChangeAvatarExtensionAsync(Guid userId, AvatarFileTypes.ValidAvatarExtensions? newExtension)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");
        user.AvatarExtension = newExtension;
        await context.SaveChangesAsync();
        return true;
    }
}