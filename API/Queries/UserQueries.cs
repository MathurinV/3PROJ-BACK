using System.Security.Claims;
using DAL.Models.Users;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [Authorize]
    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetCurrentUser([Service] IUserRepository userRepository,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new Exception("Issue with getting user id");
        return userRepository.GetById(Guid.Parse(userId));
    }

    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetUserById([Service] IUserRepository userRepository, Guid id)
    {
        return userRepository.GetById(id);
    }

    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppUser?> GetUserByEmail([Service] IUserRepository userRepository, string email = null!)
    {
        return userRepository.GetByEmail(email);
    }
}