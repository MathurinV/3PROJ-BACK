using DAL.Models.Expenses;
using DAL.Repositories;
using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Path = System.IO.Path;

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

        var fileExtension = Path.GetExtension(file.FileName);
        var validFileExtension = JustificationFileTypes.StringToValidJustificationExtension(fileExtension);

        var ftpCLient = new AsyncFtpClient("ftp", DockerEnv.FtpJustificationsUser, DockerEnv.FtpJustificationsPassword);
        await ftpCLient.AutoConnect();

        if (expense.JustificationExtension != null)
        {
            var fileNameWithExtensionToDelete =
                $"{expenseIdString}{JustificationFileTypes.ValidJustificationExtensionToString(expense.JustificationExtension)}";
            await ftpCLient.DeleteFile(fileNameWithExtensionToDelete);
            await expenseRepository.ChangeExpenseJustificationExtensionAsync(expenseId, null);
        }

        var stream = file.OpenReadStream();
        var fileNameWithExtension =
            $"{expenseIdString}{JustificationFileTypes.ValidJustificationExtensionToString(validFileExtension)}";
        var status = await ftpCLient.UploadStream(stream, fileNameWithExtension);

        await ftpCLient.Disconnect();

        switch (status)
        {
            case FtpStatus.Failed:
                return BadRequest("Failed to upload file");
            case FtpStatus.Success:
                await expenseRepository.ChangeExpenseJustificationExtensionAsync(expenseId,
                    JustificationFileTypes.StringToValidJustificationExtension(fileExtension));
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