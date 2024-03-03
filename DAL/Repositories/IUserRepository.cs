using DAL.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace DAL.Repositories;

public interface IUserRepository
{
    Task<AppUser?> InsertAsync(AppUserInsertDto appUserInsertDto);
    Task<SignInResult> SignInAsync(AppUserLoginDto appUserLoginDto);
    Task<bool> SignOutAsync();
    Task<bool> AddToBalanceAsync(Guid userId, decimal amount);
    IQueryable<AppUser> GetAll();
    IQueryable<AppUser?> GetById(Guid id);
    IQueryable<AppUser?> GetByEmail(string email = null!);
}