using API.Utilities;
using DAL;
using DAL.Repositories;
using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

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
        var status = await UploadImage.PostImage(UploadImage.UploadType.Group, token, file, distributedCache,
            null, groupRepository, null);

        switch (status)
        {
            case FtpStatus.Failed:
                return BadRequest("Failed to upload file");
            case FtpStatus.Success:
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