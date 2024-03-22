using System.Security.Claims;
using DAL.Models.Expenses;
using DAL.Repositories;
using HotChocolate.Authorization;
using HotChocolate.Language;
using Microsoft.AspNetCore.Mvc;

namespace API.Queries;

[ExtendObjectType("Query")]
public class ExpenseQueries
{
    [Authorize]
    public async Task<ICollection<KeyValuePair<Guid, decimal>>> PrevisualizeUserExpenses(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IUserRepository userRepository,
        [FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        ExpensePrevisualizationInput expensePrevisualizationInput)
    {
        var creatorStringId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (creatorStringId == null) throw new Exception("Could not find user");
        var creatorId = Guid.Parse(creatorStringId);
        var creator = await userRepository.GetByIdAsync(creatorId);
        if (creator == null) throw new Exception("Creator not found");

        var group = await groupRepository.GetByIdAsync(expensePrevisualizationInput.GroupId);
        if (group == null) throw new Exception("Group not found");

        var userAmountsList = expensePrevisualizationInput.UserAmountsList;

        // Checks if all the weighted users are in the group
        if (!await userGroupRepository.AreUsersInGroup(expensePrevisualizationInput.GroupId,
                userAmountsList.Select(x => x.Key)))
            throw new Exception("Not all the weighted users are in the group");

        // var creatorInUserAmountsList =
        //     userAmountsList.FirstOrDefault(x => x.Key == creatorId);

        var totalAmount = Math.Round(expensePrevisualizationInput.Amount, 2);
        var numberOfUsersWithoutSpecifiedAmount = 0;
        var userIdsWithoutSpecifiedAmount = new List<Guid>();

        var userIdsWithAmounts = new List<KeyValuePair<Guid, decimal>>();

        foreach (var keyValuePair in userAmountsList)
        {
            if (keyValuePair.Value.HasValue)
            {
                var currentUserAmount = Math.Round(keyValuePair.Value.Value, 2);
                userIdsWithAmounts.Add(new KeyValuePair<Guid, decimal>(keyValuePair.Key, currentUserAmount));
                totalAmount -= currentUserAmount;
                if (totalAmount < 0)
                    throw new Exception("The sum of all the users' amounts is greater than the total amount");
            }
            else
            {
                numberOfUsersWithoutSpecifiedAmount += 1;
                userIdsWithoutSpecifiedAmount.Add(keyValuePair.Key);
            }
        }

        // At this point, the list of users has been explored, and we know the amount left to be paid by the users without a specified amount
        if (numberOfUsersWithoutSpecifiedAmount > 0)
        {
            var toBePaidByUserAmount = Math.Round(totalAmount / numberOfUsersWithoutSpecifiedAmount, 2);
            foreach (var id in userIdsWithoutSpecifiedAmount)
            {
                userIdsWithAmounts.Add(new KeyValuePair<Guid, decimal>(id, toBePaidByUserAmount));
            }
        }

        // Removes the expense creator from the list if he is included in the expensePrevisualizationInput
        var isCreatorInUserIdsWithAmounts = userIdsWithAmounts.FirstOrDefault(x => x.Key == creatorId);
        // /!\
        if (isCreatorInUserIdsWithAmounts.Key != default)
        {
            userIdsWithAmounts.Remove(isCreatorInUserIdsWithAmounts);
        }

        return userIdsWithAmounts;
    }
}