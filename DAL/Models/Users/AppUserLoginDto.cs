using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Users;

/// <summary>
///     Data transfer object for user login details.
/// </summary>
public class AppUserLoginDto
{
    /// <summary>
    ///     Represents the username of a user.
    /// </summary>
    [StringLength(50)]
    public string Username { get; set; } = null!;

    /// <summary>
    ///     Represents the password of a user.
    /// </summary>
    [StringLength(50)]
    public string Password { get; set; } = null!;

    /// <summary>
    ///     Represents the "Remember Me" property for user login details.
    /// </summary>
    public bool RememberMe { get; set; } = false;
}