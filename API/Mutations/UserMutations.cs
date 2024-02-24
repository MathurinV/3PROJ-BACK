using System.ComponentModel.DataAnnotations;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using DAL.Repositories;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class UserMutations
{
    public async Task<AppUser?> CreateUser([FromServices] IUserRepository userRepository, [FromServices] UserManager<AppUser> userManager,
        AppUserInsertDto appUserInsertDto)
    {
        var validationContext = new ValidationContext(appUserInsertDto, null, null);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(appUserInsertDto, validationContext, validationResults, true);

        var passwordValidationResult = await userManager.PasswordValidators.First().ValidateAsync(userManager, null, appUserInsertDto.Password);
        if (!passwordValidationResult.Succeeded)
        {
            validationResults.AddRange(passwordValidationResult.Errors.Select(e => new ValidationResult(e.Description)));
        }

        if (!isValid || validationResults.Count > 0)
        {
            var errors = string.Join(", ", validationResults.Select(e => e.ErrorMessage));
            throw new Exception($"Validation failed: {errors}");
        }

        return await userRepository.InsertAsync(appUserInsertDto);
    }

    public async Task<SignInResult> SignIn([FromServices] IUserRepository userRepository,
        AppUserLoginDto appUserLoginDto)
    {
        return await userRepository.SignInAsync(appUserLoginDto);
    }
    
    [Authorize]
    public async Task<bool> SIgnOut([FromServices] IUserRepository userRepository)
    {
        return await userRepository.SIgnOutAsync();
    }
    
    [Authorize]
    public async Task<UserGroup?> JoinGroup([FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IInvitationRepository invitationRepository,
        [FromServices] IGroupRepository groupRepository,
        UserGroupInsertDto userGroupInsertDto)
    {
        if (!await invitationRepository.DeleteAsync(userGroupInsertDto.GroupId, userGroupInsertDto.UserId)) return null;
        return await userGroupRepository.InsertAsync(userGroupInsertDto);
    }
    
    [Authorize]
    public async Task<bool> PayDuesByUserId([FromServices] IUserExpenseRepository userExpenseRepository,
        Guid userId){
        return await userExpenseRepository.PayByUserId(userId);
    }
    
    [Authorize]
    public async Task<bool> AddToBalance([FromServices] IUserRepository userRepository,
        Guid userId, decimal amount)
    {
        return await userRepository.AddToBalance(userId, amount);
    }
}