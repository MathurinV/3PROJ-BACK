using API.Utilities;
using DAL.Models.Expenses;
using DAL.Repositories;
using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class RibsController: ControllerBase
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

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetRib(string fileName,
        [FromServices] IUserRepository userRepository)
    {
        var userIdString = fileName.Split('.')[0];
        var user = await userRepository.GetByIdAsync(Guid.Parse(userIdString));
        if (user == null) return NotFound($"User with ID {userIdString} not found");

        var ftpClient = new AsyncFtpClient("ftp", DockerEnv.FtpUserRibsUser, DockerEnv.FtpUserRibsPassword);
        await ftpClient.AutoConnect();
        
        var fileExtension = user.RibExtension;
        if (fileExtension == null || !await ftpClient.FileExists(fileName)) return NotFound("Rib not found");
        
        var stream = new MemoryStream();
        var status = await ftpClient.DownloadStream(stream, fileName);
        await ftpClient.Disconnect();
        if (!status) return NotFound("Rib not found");
        stream.Position = 0;
        
        var fileExtensionString = JustificationFileTypes.ValidJustificationExtensionToString(fileExtension);
        return File(stream, JustificationFileTypes.ValidJustificationExtensionsMimeType(fileExtension));
    }
}