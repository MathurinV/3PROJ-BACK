using API.GroupSumUps;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using QuestPDF.Fluent;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupSumUpsController : ControllerBase
{
    [HttpGet("{token}")]
    public async Task<IActionResult> GetGroupSumUp(string token,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IExpenseRepository expenseRepository,
        [FromServices] IDistributedCache distributedCache)
    {
        var groupIdString = await distributedCache.GetStringAsync(token);
        if (groupIdString == null) return BadRequest("Invalid token");

        var groupId = Guid.Parse(groupIdString);
        var currentGroup = groupRepository.GetById(groupId);

        var pdfDataModel = new GroupSumUpModel
        {
            Group = currentGroup
        };
        var pdfDocument = new GroupSumUpDocument(pdfDataModel);
        var pdfStream = pdfDocument.GeneratePdf();
        
        return File(pdfStream, "application/pdf");
    }
}