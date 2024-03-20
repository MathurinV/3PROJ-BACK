using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.Messages;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using HotChocolate;
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

    [GraphQLIgnore] public AvatarFileTypes.ValidAvatarExtensions? AvatarExtension { get; set; }
}

public class AvatarFileTypes
{
    public enum ValidAvatarExtensions
    {
        Jpg,
        Png,
        Jpeg
    }

    public static string ValidAvatarExtensionToString(
        ValidAvatarExtensions? validAvatarExtensions)
    {
        return validAvatarExtensions switch
        {
            ValidAvatarExtensions.Jpg => ".jpg",
            ValidAvatarExtensions.Png => ".png",
            ValidAvatarExtensions.Jpeg => ".jpeg",
            _ => throw new ArgumentOutOfRangeException(nameof(validAvatarExtensions),
                validAvatarExtensions, null)
        };
    }

    public static ValidAvatarExtensions StringToValidAvatarExtension(string avatarExtension)
    {
        return avatarExtension switch
        {
            ".jpg" => ValidAvatarExtensions.Jpg,
            ".png" => ValidAvatarExtensions.Png,
            ".jpeg" => ValidAvatarExtensions.Jpeg,
            _ => throw new ArgumentOutOfRangeException(nameof(avatarExtension), avatarExtension, null)
        };
    }
    
    public static string ValidAvatarExtensionsMimeType(ValidAvatarExtensions? validAvatarExtensions)
    {
        return validAvatarExtensions switch
        {
            ValidAvatarExtensions.Jpg => "image/jpeg",
            ValidAvatarExtensions.Png => "image/png",
            ValidAvatarExtensions.Jpeg => "image/jpeg",
            _ => throw new ArgumentOutOfRangeException(nameof(validAvatarExtensions),
                validAvatarExtensions, null)
        };
    }
}