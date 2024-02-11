using System.ComponentModel.DataAnnotations;
using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Queries;

[ExtendObjectType("Query")]
public class UserQueries
{
    // public async Task<IEnumerable<AppUser>> GetUsers([FromServices] IUserRepository userRepository)
    // {
    //     return await userRepository.GetAllAsync();
    // }
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser> GetUsers([Service] IUserRepository userRepository) =>
        userRepository.GetAll();

    public async Task<AppUser?> GetUser([FromServices] IUserRepository userRepository, Guid id)
    {
        return await userRepository.GetByIdAsync(id);
    }

    public async Task<AppUser?> GetUserByEmail([FromServices] IUserRepository userRepository,
        [StringLength(50)] [EmailAddress] string email)
    {
        return await userRepository.GetByEmailAsync(email);
    }
}