using API.Utilities;
using DAL.Models.Expenses;
using DAL.Repositories;
using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class RibsController : ControllerBase
{
    [HttpPost("{token}")]
    public async Task<ActionResult<FtpStatus>> PostRibs(string token,
        [FromForm] IFormFile file,
        [FromServices] IDistributedCache distributedCache,
        [FromServices] IUserRepository userRepository
    )
    {
        var status = await UploadImage.PostImage(UploadImage.UploadType.Rib, token, file, distributedCache,
            userRepository, null, null);

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

    [HttpGet("{token}")]
    public async Task<IActionResult> GetRib(string token,
        [FromServices] IUserRepository userRepository,
        [FromServices] IDistributedCache distributedCache)
    {
        var userIdString = await distributedCache.GetStringAsync(token);
        if (userIdString == null) return BadRequest("Invalid token");

        var userId = Guid.Parse(userIdString);

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound($"User with ID {userId} not found");

        var ftpClient = new AsyncFtpClient("ftp", DockerEnv.FtpUserRibsUser, DockerEnv.FtpUserRibsPassword);
        await ftpClient.AutoConnect();

        var extension = user.RibExtension;
        var extensionString = JustificationFileTypes.ValidJustificationExtensionToString(extension);

        var stream = new MemoryStream();
        var fileNameWithExtension = $"{userIdString}{extensionString}";
        var status = await ftpClient.DownloadStream(stream, fileNameWithExtension);
        await ftpClient.Disconnect();
        if (!status) return NotFound("Rib not found");
        stream.Position = 0;

        return File(stream, JustificationFileTypes.ValidJustificationExtensionsMimeType(extension));
    }
}