using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Users;

/// <summary>
/// Represents a DTO (Data Transfer Object) class for inserting a new AppUser.
/// </summary>
public class AppUserInsertDto
{
    /// <summary>
    /// Represents the username of an application user.
    /// </summary>
    [StringLength(50)]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Represents an email address.
    /// </summary>
    [EmailAddress]
    [StringLength(50)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Represents the password property.
    /// </summary>
    [StringLength(50)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Represents a user role.
    /// </summary>
    [StringLength(50)]
    public string Role { get; set; } = null!;

    /// <summary>
    /// Converts an <see cref="AppUserInsertDto"/> to an <see cref="AppUser"/> object.
    /// </summary>
    /// <returns>The converted <see cref="AppUser"/> object.</returns>
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