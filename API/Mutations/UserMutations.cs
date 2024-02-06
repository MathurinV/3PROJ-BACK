using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Mutations;

[ExtendObjectType(Name = "Mutation")]
public class UserMutations
{
    public async Task<AppUser?> CreateUser([FromServices] IUserRepository userRepository,
        AppUserInsertDto appUserInsertDto)
    {
        return await userRepository.InsertAsync(appUserInsertDto);
    }
}