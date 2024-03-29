using DAL;
using DAL.Repositories;
using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Path = System.IO.Path;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class AvatarsController : ControllerBase
{
    [HttpPost("{token}")]
    public async Task<ActionResult<FtpStatus>> PostAvatar(string token,
        [FromForm] IFormFile file,
        [FromServices] IDistributedCache distributedCache,
        [FromServices] IUserRepository userRepository)
    {
        var userIdString = await distributedCache.GetStringAsync(token) ?? throw new Exception("Token not found");

        var userId = Guid.Parse(userIdString);
        var user = await userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");

        var fileExtension = Path.GetExtension(file.FileName);
        var validFileExtension = ImageFileTypes.StringToValidImageExtension(fileExtension);

        var ftpClient = new AsyncFtpClient("ftp", DockerEnv.FtpAvatarsUser, DockerEnv.FtpAvatarsPassword);
        await ftpClient.AutoConnect();

        if (user.AvatarExtension != null)
        {
            var fileNameWithExtensionToDelete =
                $"{userIdString}{ImageFileTypes.ValidImageExtensionToString(user.AvatarExtension)}";
            await ftpClient.DeleteFile(fileNameWithExtensionToDelete);
            await userRepository.ChangeAvatarExtensionAsync(userId, null);
        }

        var stream = file.OpenReadStream();
        var fileNameWithExtension =
            $"{userIdString}{ImageFileTypes.ValidImageExtensionToString(validFileExtension)}";
        var status = await ftpClient.UploadStream(stream, fileNameWithExtension);

        await ftpClient.Disconnect();

        switch (status)
        {
            case FtpStatus.Failed:
                return BadRequest("Failed to upload file");
            case FtpStatus.Success:
                await userRepository.ChangeAvatarExtensionAsync(userId,
                    ImageFileTypes.StringToValidImageExtension(fileExtension));
                return Ok("File uploaded successfully");
            default:
                return BadRequest("Unknown error");
        }
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetAvatar(string fileName,
        [FromServices] IUserRepository userRepository)
    {
        var userIdString = fileName.Split('.')[0];
        var user = await userRepository.GetByIdAsync(Guid.Parse(userIdString));
        if (user == null) return NotFound($"User with ID {userIdString} not found");

        var ftpClient = new AsyncFtpClient("ftp", DockerEnv.FtpAvatarsUser, DockerEnv.FtpAvatarsPassword);
        await ftpClient.AutoConnect();

        var fileExtension = user.AvatarExtension;
        if (fileExtension == null || !await ftpClient.FileExists(fileName)) return NotFound("Avatar not found");

        var stream = new MemoryStream();
        var status = await ftpClient.DownloadStream(stream, fileName);
        await ftpClient.Disconnect();
        if (!status) return NotFound("Avatar not found");
        stream.Position = 0;

        var fileExtensionString = ImageFileTypes.ValidImageExtensionToString(fileExtension);
        return File(stream, ImageFileTypes.ValidImageExtensionsMimeType(fileExtension));
    }
}