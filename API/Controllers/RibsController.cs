using API.Utilities;
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
}