using API.SumUps.GroupSumUps;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [FromServices] IDistributedCache distributedCache)
    {
        var groupIdString = await distributedCache.GetStringAsync(token);
        if (groupIdString == null) return BadRequest("Invalid token");

        var groupId = Guid.Parse(groupIdString);
        var currentGroup = await groupRepository.GetByIdAsync(groupId);
        if (currentGroup == null) return NotFound($"Group with ID {groupId} not found");

        List<GroupSumUpExpense> groupSumUpExpenses = new();
        var queryableGroup = groupRepository.GetById(groupId).Include(g => g!.Expenses).ThenInclude(e => e.UserExpenses)
            .ThenInclude(ue => ue.User).ThenInclude(u => u.UserGroups).ThenInclude(ug => ug.PayTo);

        foreach (var expense in queryableGroup.SelectMany(g => g.Expenses))
        {
            var groupSumUpExpense = new GroupSumUpExpense
            {
                Name = expense.Description,
                Amount = expense.Amount,
                Description = expense.Description,
                Date = expense.CreatedAt,
                Type = expense.ExpenseType
            };
            groupSumUpExpenses.Add(groupSumUpExpense);
        }

        var pdfDataModel = new GroupSumUpModel
        {
            GroupName = currentGroup.Name,
            Expenses = groupSumUpExpenses,
            Payers = new List<GroupSumUpUser>()
        };


        var usergroups = queryableGroup.SelectMany(group => group!.UserGroups).Include(group => group.PayTo)
            .Include(ug => ug.User).ToList();
        foreach (var userGroup in usergroups)
        {
            var user = userGroup.User;
            var payTo = userGroup.PayTo;
            var groupSumUpUser = new GroupSumUpUser
            {
                UserName = user.UserName,
                AmountDue = payTo?.AmountToPay,
                ToUserName = payTo?.PayToUser?.UserName
            };
            pdfDataModel.Payers.Add(groupSumUpUser);
        }

        var pdfDocument = new GroupSumUpDocument(pdfDataModel);
        var pdfStream = pdfDocument.GeneratePdf();

        return File(pdfStream, "application/pdf");
    }
}