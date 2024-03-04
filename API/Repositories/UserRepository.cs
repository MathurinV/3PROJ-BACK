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
        if (!result.Succeeded) return null;
        if (!await roleManager.RoleExistsAsync(appUserInsertDto.Role)) return null;
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
        if (user == null) return false;
        user.Balance += amount;
        await context.SaveChangesAsync();
        return true;
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var user = context.Users.Find(id);
        if (user == null) return Task.FromResult(false);
        context.Users.Remove(user);
        if (context.SaveChanges() == 0) return Task.FromResult(false);
        return Task.FromResult(true);
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
}