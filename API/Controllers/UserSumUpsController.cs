using API.SumUps.UserSumUps;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using QuestPDF.Fluent;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserSumUpsController: ControllerBase
{
    [HttpGet("{token}")]
    public async Task<IActionResult> GetUserSumUp(string token,
        [FromServices] IUserRepository userRepository,
        [FromServices] IDistributedCache distributedCache)
    {
        var userIdString = await distributedCache.GetStringAsync(token);
        if (userIdString == null) return BadRequest("Invalid token");
        
        var userId = Guid.Parse(userIdString);
        var currentUser = await userRepository.GetByIdAsync(userId);
        if (currentUser == null) return NotFound($"User with ID {userId} not found");
        
        ICollection<GroupInfos> groupInfosCollection = new List<GroupInfos>();
        var queryableUser = userRepository.GetById(userId);
        var userGroups = queryableUser.SelectMany(u => u.UserGroups).Include(ug => ug.Group).Include(ug => ug.PayTo).ThenInclude(to => to.PayToUser).ToList();
        foreach (var userGroup in userGroups)
        {
            groupInfosCollection.Add(new GroupInfos
            {
                GroupName = userGroup.Group.Name,
                Balance = userGroup.Balance,
                AmountDue = userGroup.PayTo.AmountToPay,
                ToUserName = userGroup.PayTo.PayToUser?.UserName
            });
        }

        var pdfDataModel = new UserSumUpModel
        {
            UserName = currentUser.UserName,
            Email = currentUser.Email,
            GroupInfos = groupInfosCollection
        };
        var pdfDocument = new UserSumUpDocument(pdfDataModel);
        var pdfStream = pdfDocument.GeneratePdf();
        
        return File(pdfStream, "application/pdf");
    }
}