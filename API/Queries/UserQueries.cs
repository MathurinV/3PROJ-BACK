using DAL.Models.Users;
using DAL.Repositories;

namespace API.Queries;

[ExtendObjectType("Query")]
public class UserQueries
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser> GetUsers([Service] IUserRepository userRepository)
    {
        return userRepository.GetAll();
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetUserById([Service] IUserRepository userRepository, Guid id)
    {
        return userRepository.GetById(id);
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetUserByEmail([Service] IUserRepository userRepository, string email = null!)
    {
        return userRepository.GetByEmail(email);
    }
}