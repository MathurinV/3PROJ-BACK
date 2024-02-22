using DAL.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace DAL.Repositories;

public interface IUserRepository
{
    public Task<AppUser?> InsertAsync(AppUserInsertDto appUserInsertDto);
    public Task<SignInResult> SignInAsync(AppUserLoginDto appUserLoginDto);
    public Task<bool> SIgnOutAsync();
    public Task<bool> AddToBalance(Guid userId, decimal amount);
    IQueryable<AppUser> GetAll();
    IQueryable<AppUser?> GetById(Guid id);
    IQueryable<AppUser?> GetByEmail(string email = null!);
}