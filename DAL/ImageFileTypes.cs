namespace DAL;

public class ImageFileTypes
{
    public enum ValidImageExtensions
    {
        Jpg,
        Png,
        Jpeg
    }

    public static string ValidImageExtensionToString(
        ValidImageExtensions? validImageExtensions)
    {
        return validImageExtensions switch
        {
            ValidImageExtensions.Jpg => ".jpg",
            ValidImageExtensions.Png => ".png",
            ValidImageExtensions.Jpeg => ".jpeg",
            _ => throw new ArgumentOutOfRangeException(nameof(validImageExtensions),
                validImageExtensions, null)
        };
    }

    public static ValidImageExtensions StringToValidImageExtension(string imageExtension)
    {
        return imageExtension switch
        {
            ".jpg" => ValidImageExtensions.Jpg,
            ".png" => ValidImageExtensions.Png,
            ".jpeg" => ValidImageExtensions.Jpeg,
            _ => throw new ArgumentOutOfRangeException(nameof(imageExtension), imageExtension, null)
        };
    }

    public static string ValidImageExtensionsMimeType(ValidImageExtensions? validImageExtensions)
    {
        return validImageExtensions switch
        {
            ValidImageExtensions.Jpg => "image/jpeg",
            ValidImageExtensions.Png => "image/png",
            ValidImageExtensions.Jpeg => "image/jpeg",
            _ => throw new ArgumentOutOfRangeException(nameof(validImageExtensions),
                validImageExtensions, null)
        };
    }
}