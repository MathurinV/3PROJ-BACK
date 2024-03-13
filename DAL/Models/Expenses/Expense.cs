using System.ComponentModel.DataAnnotations;
using DAL.Models.Groups;
using DAL.Models.UserExpenses;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.Expenses;

public class Expense
{
    public Guid Id { get; set; }
    [GraphQLIgnore] public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public decimal Amount { get; set; }
    [StringLength(255)] public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    [GraphQLIgnore] public Guid CreatedById { get; set; }
    public AppUser CreatedBy { get; set; } = null!;
    public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();

    [GraphQLIgnore]
    public JustificationFileTypes.ValidJustificationExtensions? JustificationExtension { get; set; } = null;
}

public class JustificationFileTypes
{
    public enum ValidJustificationExtensions
    {
        Pdf,
        Jpg,
        Png,
        Jpeg
    }

    public static string ValidJustificationExtensionToString(ValidJustificationExtensions? justificationExtension)
    {
        return justificationExtension switch
        {
            ValidJustificationExtensions.Pdf => ".pdf",
            ValidJustificationExtensions.Jpg => ".jpg",
            ValidJustificationExtensions.Png => ".png",
            ValidJustificationExtensions.Jpeg => ".jpeg",
            null => "",
            _ => throw new ArgumentOutOfRangeException(nameof(justificationExtension), justificationExtension, null)
        };
    }

    public static ValidJustificationExtensions? StringToValidJustificationExtension(string? justificationExtension)
    {
        return justificationExtension switch
        {
            ".pdf" => ValidJustificationExtensions.Pdf,
            ".jpg" => ValidJustificationExtensions.Jpg,
            ".png" => ValidJustificationExtensions.Png,
            ".jpeg" => ValidJustificationExtensions.Jpeg,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(justificationExtension), justificationExtension, null)
        };
    }
}