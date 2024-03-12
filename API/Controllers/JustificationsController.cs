using DAL.Repositories;
using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class JustificationsController : ControllerBase
{
    [HttpPost("{token}")]
    public async Task<ActionResult<FtpStatus>> PostJustifications(string token,
        [FromForm] IFormFile file,
        [FromServices] IDistributedCache distributedCache,
        [FromServices] IExpenseRepository expenseRepository)
    {
        var expenseIdString = await distributedCache.GetStringAsync(token);
        if (expenseIdString == null) return BadRequest("Invalid token");

        var expenseId = Guid.Parse(expenseIdString);

        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null) return NotFound($"Expense with ID {expenseId} not found");

        var ftpCLient = new AsyncFtpClient("ftp", DockerEnv.FtpUser, DockerEnv.FtpPassword);
        await ftpCLient.AutoConnect();

        var stream = file.OpenReadStream();
        var status = await ftpCLient.UploadStream(stream, $"/justifications/{expenseId}", FtpRemoteExists.Overwrite, createRemoteDir:true);

        await ftpCLient.Disconnect();
        return Ok(status);
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetJustification(string token,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IDistributedCache distributedCache)
    {
        var expenseIdString = await distributedCache.GetStringAsync(token);
        if (expenseIdString == null) return BadRequest("Invalid token");

        var expenseId = Guid.Parse(expenseIdString);

        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null) return NotFound($"Expense with ID {expenseId} not found");

        var ftpCLient = new AsyncFtpClient("ftp", DockerEnv.FtpUser, DockerEnv.FtpPassword);
        await ftpCLient.AutoConnect();

        var stream = new MemoryStream();
        var status = await ftpCLient.DownloadStream(stream, $"/justifications/{expenseId}");
        await ftpCLient.Disconnect();
        if (!status) return NotFound("Justification not found");
        stream.Position = 0;

        return File(stream, "application/octet-stream");
    }
}