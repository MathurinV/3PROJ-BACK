using DAL.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace DAL.Repositories;

public interface IUserRepository
{
    public Task<ICollection<AppUser>> GetAllAsync();
    public Task<AppUser?> GetByIdAsync(Guid id);
    public Task<AppUser?> GetByEmailAsync(string email);
    public Task<AppUser?> InsertAsync(AppUserInsertDto appUserInsertDto);
    public Task<SignInResult> SignInAsync(AppUserLoginDto appUserLoginDto);
    IQueryable<AppUser> GetAll();
}