using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Users;

public class AppUserInsertDto
{
    [StringLength(50)] public string UserName { get; set; } = null!;
    [EmailAddress] [StringLength(50)] public string Email { get; set; } = null!;
    [StringLength(50)] public string Password { get; set; } = null!;
    [StringLength(50)] public string Role { get; set; } = null!;

    public AppUser ToAppUser()
    {
        return new AppUser
        {
            UserName = UserName,
            Email = Email,
            EmailConfirmed = true
        };
    }
}