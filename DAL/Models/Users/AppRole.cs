using Microsoft.AspNetCore.Identity;

namespace DAL.Models.Users;

/// <summary>
/// Represents an application role. Inherits from <see cref="IdentityRole"/>.
/// </summary>
public class AppRole : IdentityRole<Guid>
{
}