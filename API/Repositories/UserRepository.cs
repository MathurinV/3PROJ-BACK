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
    public async Task<ICollection<AppUser>> GetAllAsync()
    {
        return await context.Users
            .Include(u => u.UserGroups)
            .Include(u => u.OwnedGroups)
            .Include(u => u.ReceivedMessages)
            .Include(u => u.SentMessages)
            .Include(u => u.SentGroupMessages)
            .Include(u => u.UserExpenses)
            .ThenInclude(ue => ue.Expense)
            .ToListAsync();
    }

    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .Include(u => u.UserGroups)
            .Include(u => u.OwnedGroups)
            .Include(u => u.ReceivedMessages)
            .Include(u => u.SentMessages)
            .Include(u => u.SentGroupMessages)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await context.Users
            .Include(u => u.UserGroups)
            .Include(u => u.OwnedGroups)
            .Include(u => u.ReceivedMessages)
            .Include(u => u.SentMessages)
            .Include(u => u.SentGroupMessages)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

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

    public IQueryable<AppUser> GetAll()
    {
        return context.Users;
    }
}