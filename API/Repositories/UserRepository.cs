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
    private readonly MoneyMinderDbContext _context = context;
    private readonly RoleManager<AppRole> _roleManager = roleManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<ICollection<AppUser>> GetAllAsync()
    {
        return await _context.Users
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
        return await _context.Users
            .Include(u => u.UserGroups)
            .Include(u => u.OwnedGroups)
            .Include(u => u.ReceivedMessages)
            .Include(u => u.SentMessages)
            .Include(u => u.SentGroupMessages)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
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

        var result = await _userManager.CreateAsync(appUser, appUserInsertDto.Password);
        if (!result.Succeeded) return null;
        if (!await _roleManager.RoleExistsAsync(appUserInsertDto.Role)) return null;
        await _userManager.AddToRoleAsync(appUser, appUserInsertDto.Role);
        return appUser;
    }

    public async Task<SignInResult> SignInAsync(AppUserLoginDto appUserLoginDto)
    {
        return await _signInManager.PasswordSignInAsync(appUserLoginDto.Email, appUserLoginDto.Password,
            appUserLoginDto.RememberMe, false);
    }
}