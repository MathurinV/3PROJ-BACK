using System.ComponentModel.DataAnnotations;
using DAL.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace API.ErrorsHandling.UsersHandling;

public static class AppUserModifyDtoHandling
{
    public static void ValidateAppUserModifyDto(
        UserManager<AppUser> userManager,
        AppUserModifyDto appUserModifyDto)
    {
        var validationContext = new ValidationContext(appUserModifyDto);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(appUserModifyDto, validationContext, validationResults, true);

        if (appUserModifyDto.Password != null)
        {
            var passwordValidationResult = userManager.PasswordValidators.First()
                .ValidateAsync(userManager, null, appUserModifyDto.Password).Result;
            if (!passwordValidationResult.Succeeded)
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