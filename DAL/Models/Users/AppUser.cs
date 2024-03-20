using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.Messages;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using Microsoft.AspNetCore.Identity;

namespace DAL.Models.Users;

/// <summary>
///     Represents an application user. Inherits from <see cref="IdentityUser" />.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    /// <summary>
    ///     Represents the balance of an application user. Default value is 0.
    /// </summary>
    public decimal Balance { get; set; } = 0;

    /// <summary>
    ///     Represents a collection of user groups associated with an application user. Default value is an empty list.
    /// </summary>
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    /// <summary>
    ///     Gets or sets the collection of owned groups by the user. Default value is an empty list.
    /// </summary>
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();

    /// <summary>
    ///     Represents the collection of sent messages for an application user. Default value is an empty list.
    /// </summary>
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();

    /// <summary>
    ///     Gets or sets the collection of received messages for an AppUser. Default value is an empty list.
    /// </summary>
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

    /// <summary>
    ///     Represents the collection of group messages sent by an application user. Default value is an empty list.
    /// </summary>
    public ICollection<GroupMessage> SentGroupMessages { get; set; } = new List<GroupMessage>();

    /// <summary>
    ///     Represents the collection of expenses made by a user. Default value is an empty list.
    /// </summary>
    public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();

    /// <summary>
    ///     Represents the collection of expenses created by a user. Default value is an empty list.
    /// </summary>
    public ICollection<Expense> CreatedExpenses { get; set; } = new List<Expense>();

    /// <summary>
    ///     Represents the collection of invitations sent by a user. Default value is an empty list.
    /// </summary>

    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public ProfilePictureFileTypes.ValidProfilePictureExtensions? ValidProfilePictureExtensions { get; set; }
}

public class ProfilePictureFileTypes
{
    public enum ValidProfilePictureExtensions
    {
        Jpg,
        Png,
        Jpeg
    }

    public static string ValidProfilePictureExtensionToString(
        ValidProfilePictureExtensions? validProfilePictureExtensions)
    {
        return validProfilePictureExtensions switch
        {
            ValidProfilePictureExtensions.Jpg => ".jpg",
            ValidProfilePictureExtensions.Png => ".png",
            ValidProfilePictureExtensions.Jpeg => ".jpeg",
            null => "",
            _ => throw new ArgumentOutOfRangeException(nameof(validProfilePictureExtensions),
                validProfilePictureExtensions, null)
        };
    }

    public static ValidProfilePictureExtensions? StringToValidProfilePictureExtension(string? profilePictureExtension)
    {
        return profilePictureExtension switch
        {
            ".jpg" => ValidProfilePictureExtensions.Jpg,
            ".png" => ValidProfilePictureExtensions.Png,
            ".jpeg" => ValidProfilePictureExtensions.Jpeg,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(profilePictureExtension), profilePictureExtension, null)
        };
    }
}