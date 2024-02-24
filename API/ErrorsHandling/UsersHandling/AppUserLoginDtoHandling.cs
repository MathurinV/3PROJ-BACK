using System.ComponentModel.DataAnnotations;
using DAL.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace API.ErrorsHandling.UsersHandling;

public static class AppUserLoginDtoHandling
{
    public static void ValidateAppUserLoginDto(
        UserManager<AppUser> userManager
        , AppUserLoginDto appUserInsertDto)
    {
        var validationContext = new ValidationContext(appUserInsertDto);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(appUserInsertDto, validationContext, validationResults, true);

        var passwordValidationResult = userManager.PasswordValidators.First()
            .ValidateAsync(userManager, null, appUserInsertDto.Password).Result;
        if (!passwordValidationResult.Succeeded)
        {
            validationResults.AddRange(
                passwordValidationResult.Errors.Select(e => new ValidationResult(e.Description)));
        }

        if (!isValid || validationResults.Count > 0)
        {
            var errors = string.Join(", ", validationResults.Select(e => e.ErrorMessage));
            throw new Exception($"Validation failed: {errors}");
        }
    }
}