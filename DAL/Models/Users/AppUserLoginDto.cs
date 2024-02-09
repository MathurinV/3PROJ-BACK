using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Users;

public class AppUserLoginDto
{
    [StringLength(50)] public string Username { get; set; } = null!;
    [StringLength(50)] public string Password { get; set; } = null!;
    public bool RememberMe { get; set; } = false;
}