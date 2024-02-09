using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Mutations;

[ExtendObjectType("Mutation")]
public class GroupMutations
{
    public async Task<Group?> CreateGroup([FromServices] IGroupRepository groupRepository,
        [FromServices] IUserGroupRepository userGroupRepository,
        GroupInsertDto groupInsertDto)
    {
        var currentGroup = await groupRepository.InsertAsync(groupInsertDto);
        if (currentGroup == null) return null;
        var userGroup = new UserGroupInsertDto
        {
            UserId = currentGroup.OwnerId,
            GroupId = currentGroup.Id
        };
        await userGroupRepository.InsertAsync(userGroup);
        return await groupRepository.GetByIdAsync(currentGroup.Id);
    }

    public async Task<Group?> JoinGroup([FromServices] IUserGroupRepository userGroupRepository,
        [FromServices] IGroupRepository groupRepository,
        UserGroupInsertDto userGroupInsertDto)
    {
        var userGroup = new UserGroupInsertDto
        {
            UserId = userGroupInsertDto.UserId,
            GroupId = userGroupInsertDto.GroupId
        };
        await userGroupRepository.InsertAsync(userGroup);
        return await groupRepository.GetByIdAsync(userGroupInsertDto.GroupId);
    }

    public async Task<ICollection<UserExpense?>> AddUserExpenses(
        [FromServices]IUserExpenseRepository userExpenseRepository,
        [FromServices]IExpenseRepository expenseRepository,
        ExpenseInsertDto expenseInsertDto
    )
    {
        var expense = await expenseRepository.InsertAsync(expenseInsertDto);
        if (expense == null) return [];
        var totalWeight = expenseInsertDto.WeightedUsers.Sum(x => x.Value);
        var userExpenses = new List<UserExpenseInsertDto>();
        
        foreach (var (userId, weight) in expenseInsertDto.WeightedUsers)
        {
            userExpenses.Add(new UserExpenseInsertDto
            {
                ExpenseId = expense.Id,
                UserId = userId,
                Amount = expense.Amount * weight / totalWeight
            });
        }
        return await userExpenseRepository.InsertManyAsync(userExpenses);
    }
}