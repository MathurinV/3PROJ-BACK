using API.Utilities;
using DAL.Models.Expenses;
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
        var status = await UploadImage.PostImage(UploadImage.UploadType.Justification, token, file, distributedCache,
            null, null, expenseRepository);


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
    public async Task<IActionResult> GetJustification(string token,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IDistributedCache distributedCache)
    {
        var expenseIdString = await distributedCache.GetStringAsync(token);
        if (expenseIdString == null) return BadRequest("Invalid token");

        var expenseId = Guid.Parse(expenseIdString);

        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null) return NotFound($"Expense with ID {expenseId} not found");

        var ftpCLient = new AsyncFtpClient("ftp", DockerEnv.FtpJustificationsUser, DockerEnv.FtpJustificationsPassword);
        await ftpCLient.AutoConnect();

        var extension = expense.JustificationExtension;
        var extensionString = JustificationFileTypes.ValidJustificationExtensionToString(extension);

        var stream = new MemoryStream();
        var fileNameWithExtension = $"{expenseIdString}{extensionString}";
        var status = await ftpCLient.DownloadStream(stream, fileNameWithExtension);
        await ftpCLient.Disconnect();
        if (!status) return NotFound("Justification not found");
        stream.Position = 0;

        return File(stream, JustificationFileTypes.ValidJustificationExtensionsMimeType(extension));
    }
}