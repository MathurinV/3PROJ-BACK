using DAL;
using DAL.Models.Expenses;
using DAL.Repositories;
using FluentFTP;
using Microsoft.Extensions.Caching.Distributed;
using Path = System.IO.Path;

namespace API.Utilities;

public static class UploadImage
{
    public enum UploadType
    {
        Avatar,
        Group,
        Justification
    }

    public static async Task<FtpStatus> PostImage(
        UploadType uploadType,
        string token,
        IFormFile file,
        IDistributedCache distributedCache,
        IUserRepository? userRepository,
        IGroupRepository? groupRepository,
        IExpenseRepository? expenseRepository
    )
    {
        var entityIdString = await distributedCache.GetStringAsync(token) ??
                             throw new Exception("Token not found");
        var entityId = Guid.Parse(entityIdString);

        var fileExtension = Path.GetExtension(file.FileName);
        var fileNameWithExtension = "";
        var ftpUser = "";
        var ftpPassword = "";
        string? fileNameWithExtensionToDelete = null;

        var shouldDeletePreviousFile = false;

        switch (uploadType)
        {
            case UploadType.Avatar:
            {
                if (userRepository == null)
                    throw new Exception("Can't upload avatar without user repository");
                var user = await userRepository.GetByIdAsync(entityId) ??
                           throw new Exception("User not found");
                var validFileExtension =
                    ImageFileTypes.StringToValidImageExtension(fileExtension);
                fileNameWithExtension =
                    $"{entityIdString}{ImageFileTypes.ValidImageExtensionToString(validFileExtension)}";
                ftpUser = DockerEnv.FtpAvatarsUser;
                ftpPassword = DockerEnv.FtpAvatarsPassword;
                if (user.AvatarExtension != null)
                {
                    fileNameWithExtensionToDelete =
                        $"{entityIdString}{ImageFileTypes.ValidImageExtensionToString(user.AvatarExtension)}";
                    await userRepository.ChangeAvatarExtensionAsync(entityId, null);
                    shouldDeletePreviousFile = true;
                }

                break;
            }
            case UploadType.Group:
            {
                if (groupRepository == null)
                    throw new Exception("Can't upload group image without group repository");
                var group = await groupRepository.GetByIdAsync(entityId) ??
                            throw new Exception("Group not found");
                var validFileExtension =
                    ImageFileTypes.StringToValidImageExtension(fileExtension);
                fileNameWithExtension =
                    $"{entityIdString}{ImageFileTypes.ValidImageExtensionToString(validFileExtension)}";
                ftpUser = DockerEnv.FtpGroupsImagesUser;
                ftpPassword = DockerEnv.FtpGroupsImagesPassword;
                if (group.ImageExtension != null)
                {
                    fileNameWithExtensionToDelete =
                        $"{entityIdString}{ImageFileTypes.ValidImageExtensionToString(group.ImageExtension)}";
                    await groupRepository.ChangeGroupImageExtensionAsync(entityId, null);
                    shouldDeletePreviousFile = true;
                }

                break;
            }
            case UploadType.Justification:
            {
                if (expenseRepository == null)
                    throw new Exception("Can't upload justification without expense repository");
                var expense = await expenseRepository.GetByIdAsync(entityId) ??
                              throw new Exception("Expense not found");
                var validFileExtension =
                    JustificationFileTypes.StringToValidJustificationExtension(fileExtension);
                fileNameWithExtension =
                    $"{entityIdString}{JustificationFileTypes.ValidJustificationExtensionToString(validFileExtension)}";
                ftpUser = DockerEnv.FtpJustificationsUser;
                ftpPassword = DockerEnv.FtpJustificationsPassword;
                if (expense.JustificationExtension != null)
                {
                    fileNameWithExtensionToDelete =
                        $"{entityIdString}{JustificationFileTypes.ValidJustificationExtensionToString(expense.JustificationExtension)}";
                    await expenseRepository.ChangeExpenseJustificationExtensionAsync(entityId, null);
                    shouldDeletePreviousFile = true;
                }

                break;
            }
            default:
            {
                throw new Exception("Invalid upload type");
            }
        }

        var ftpClient = new AsyncFtpClient("ftp", ftpUser, ftpPassword);
        await ftpClient.AutoConnect();
        if (shouldDeletePreviousFile)
            await ftpClient.DeleteFile(fileNameWithExtensionToDelete);
        var stream = file.OpenReadStream();
        var status = await ftpClient.UploadStream(stream, fileNameWithExtension);
        await ftpClient.Disconnect();

        if (status == FtpStatus.Success)
            switch (uploadType)
            {
                case UploadType.Avatar:
                {
                    if (userRepository == null)
                        throw new Exception("Can't upload avatar without user repository");
                    await userRepository.ChangeAvatarExtensionAsync(entityId,
                        ImageFileTypes.StringToValidImageExtension(fileExtension));
                    break;
                }
                case UploadType.Group:
                {
                    if (groupRepository == null)
                        throw new Exception("Can't upload group image without group repository");
                    await groupRepository.ChangeGroupImageExtensionAsync(entityId,
                        ImageFileTypes.StringToValidImageExtension(fileExtension));
                    break;
                }
                case UploadType.Justification:
                {
                    if (expenseRepository == null)
                        throw new Exception("Can't upload justification without expense repository");
                    await expenseRepository.ChangeExpenseJustificationExtensionAsync(entityId,
                        JustificationFileTypes.StringToValidJustificationExtension(fileExtension));
                    break;
                }
            }

        return status;
    }
}