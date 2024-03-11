using System.Globalization;
using System.Text;
using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Path = System.IO.Path;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class JustificationsController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public JustificationsController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public ActionResult<string> GetJustifications()
    {
        return "GET request to /api/justifications";
    }

    [HttpPost("{hashedExpenseId}")]
    public async Task<ActionResult<string>> PostJustifications(string hashedExpenseId,
        [FromForm] IFormFile file,
        [FromServices] IJustificationRepository justificationRepository,
        [FromServices] IExpenseRepository expenseRepository) // Inject the expense repository
    {
        var decodedExpenseId = Encoding.UTF8.GetString(Convert.FromBase64String(hashedExpenseId));

        var parts = decodedExpenseId.Split("--");
        var expenseId = Guid.Parse(parts[0]);
        var dateTime = DateTime.Parse(parts[1]);

        // Check if the expense exists
        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
        {
            return NotFound($"Expense with ID {expenseId} not found");
        }

        await justificationRepository.SaveJustificationAsync(expenseId, file);

        return "POST request to /api/justifications" +
               $", expenseId = {expenseId.ToString()}" +
               $", dateTime = {dateTime.ToString(CultureInfo.InvariantCulture)}" +
               $", file = {file.FileName}";
    }
    
    [HttpGet("{expenseId}")]
    public async Task<IActionResult> GetJustification(Guid expenseId,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IJustificationRepository justificationRepository)
    {
        // Check if the expense exists
        var expense = await expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
        {
            return NotFound($"Expense with ID {expenseId} not found");
        }

        // Get the justification file
        var justification = await justificationRepository.GetJustificationAsync(expenseId);
        if (justification == null)
        {
            return NotFound($"Justification for expense with ID {expenseId} not found");
        }

        // Return the file
        return File(justification.Data, justification.MimeType, justification.Name);
    }
}