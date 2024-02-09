using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class UserMutations
{
    public async Task<AppUser?> CreateUser([FromServices] IUserRepository userRepository,
        AppUserInsertDto appUserInsertDto)
    {
        return await userRepository.InsertAsync(appUserInsertDto);
    }

    public async Task<SignInResult> SignIn([FromServices] IUserRepository userRepository,
        AppUserLoginDto appUserLoginDto)
    {
        return await userRepository.SignInAsync(appUserLoginDto);
    }
}