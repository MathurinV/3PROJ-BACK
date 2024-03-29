using DAL;
using DAL.Repositories;
using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Path = System.IO.Path;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupImagesController : ControllerBase
{
    [HttpPost("{token}")]
    public async Task<ActionResult<FtpStatus>> PostGroupImage(string token,
        [FromForm] IFormFile file,
        [FromServices] IDistributedCache distributedCache,
        [FromServices] IGroupRepository groupRepository)
    {
        var groupIdString = await distributedCache.GetStringAsync(token) ?? throw new Exception("Token not found");

        var groupId = Guid.Parse(groupIdString);
        var group = await groupRepository.GetByIdAsync(groupId) ?? throw new Exception("Group not found");

        var fileExtension = Path.GetExtension(file.FileName);
        var validFileExtension = ImageFileTypes.StringToValidImageExtension(fileExtension);

        var ftpClient = new AsyncFtpClient("ftp", DockerEnv.FtpGroupsImagesUser, DockerEnv.FtpGroupsImagesPassword);
        await ftpClient.AutoConnect();

        if (group.ImageExtension != null)
        {
            var fileNameWithExtensionToDelete =
                $"{groupIdString}{ImageFileTypes.ValidImageExtensionToString(group.ImageExtension)}";
            await ftpClient.DeleteFile(fileNameWithExtensionToDelete);
            await groupRepository.ChangeGroupImageExtensionAsync(groupId, null);
        }

        var stream = file.OpenReadStream();
        var fileNameWithExtension =
            $"{groupIdString}{ImageFileTypes.ValidImageExtensionToString(validFileExtension)}";
        var status = await ftpClient.UploadStream(stream, fileNameWithExtension);

        await ftpClient.Disconnect();

        switch (status)
        {
            case FtpStatus.Failed:
                return BadRequest("Failed to upload file");
            case FtpStatus.Success:
                await groupRepository.ChangeGroupImageExtensionAsync(groupId,
                    ImageFileTypes.StringToValidImageExtension(fileExtension));
                return Ok("File uploaded successfully");
            default:
                return BadRequest("Unknown error");
        }
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetGroupImage(string fileName,
        [FromServices] IGroupRepository groupRepository)
    {
        var groupIdString = fileName.Split('.')[0];
        var group = await groupRepository.GetByIdAsync(Guid.Parse(groupIdString)) ??
                    throw new Exception("Group not found");

        var ftpClient = new AsyncFtpClient("ftp", DockerEnv.FtpGroupsImagesUser, DockerEnv.FtpGroupsImagesPassword);
        await ftpClient.AutoConnect();

        var fileExtension = group.ImageExtension;
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