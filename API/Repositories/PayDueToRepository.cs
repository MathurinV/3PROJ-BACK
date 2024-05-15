using DAL.Models.PaymentDetails;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class PayDueToRepository(MoneyMinderDbContext context) : IPayDueToRepository
{
    public async Task<ICollection<PayDueTo>> RefreshPayDueTosAsync(Guid groupId)
    {
        var userBalances = new Dictionary<Guid, decimal>();
        var groupUserExpense = await context.UserExpenses.Include(userExpense => userExpense.Expense)
            .Where(userExpense => userExpense.Expense.GroupId == groupId && userExpense.PaidAt == null)
            .ToListAsync();
        foreach (var currentUserExpense in groupUserExpense)
        {
            var currentUserBalance =
                userBalances.FirstOrDefault(userBalance => userBalance.Key == currentUserExpense.UserId);
            if (currentUserBalance.Equals(default(KeyValuePair<Guid, decimal>)))
                userBalances.Add(currentUserExpense.UserId, 0);

            userBalances[currentUserExpense.UserId] -= currentUserExpense.Amount;
        }

        var expenses = await context.Expenses.Where(expense => expense.GroupId == groupId).ToListAsync();
        foreach (var currentExpense in expenses) userBalances[currentExpense.CreatedById] += currentExpense.Amount;

        var userBalancesList = userBalances.OrderByDescending(pair => pair.Value).ToList();

        var negativeBalancesList = userBalancesList.Where(pair => pair.Value < 0).ToList();
        var positiveBalancesList = userBalancesList.Where(pair => pair.Value > 0).ToList();

        var payDueTos = new List<PayDueTo>();
        foreach (var currentBalance in negativeBalancesList)
        {
            var dueToId = positiveBalancesList.First().Key;
            payDueTos.Add(new PayDueTo
            {
                UserId = currentBalance.Key,
                AmountToPay = decimal.Abs(currentBalance.Value),
                GroupId = groupId,
                PayToUserId = dueToId
            });
            positiveBalancesList[0] = new KeyValuePair<Guid, decimal>(dueToId,
                positiveBalancesList.First().Value + currentBalance.Value);
        }

        var positiveBalancesListCount = positiveBalancesList.Count;
        for (var i = 0; i < positiveBalancesListCount - 1; i++)
            if (positiveBalancesList[i].Value < 0)
            {
                payDueTos.Add(new PayDueTo
                {
                    UserId = positiveBalancesList[i].Key,
                    AmountToPay = decimal.Abs(positiveBalancesList[i].Value),
                    GroupId = groupId,
                    PayToUserId = positiveBalancesList[i + 1].Key
                });

                positiveBalancesList[i + 1] = new KeyValuePair<Guid, decimal>(positiveBalancesList[i + 1].Key,
                    positiveBalancesList[i + 1].Value + positiveBalancesList[i].Value);
            }
            else
            {
                payDueTos.Add(new PayDueTo
                {
                    UserId = positiveBalancesList[i].Key,
                    AmountToPay = 0,
                    GroupId = groupId,
                    PayToUserId = null
                });
            }

        payDueTos.Add(new PayDueTo
        {
            UserId = positiveBalancesList[positiveBalancesListCount - 1].Key,
            AmountToPay = 0,
            GroupId = groupId,
            PayToUserId = null
        });

        foreach (var currentPayDueTo in payDueTos)
        {
            var payDueTo = await context.PayDueTos.FirstOrDefaultAsync(payDueTo =>
                payDueTo.UserId == currentPayDueTo.UserId && payDueTo.GroupId == groupId);

            if (payDueTo == null)
            {
                await context.PayDueTos.AddAsync(currentPayDueTo);
                continue;
            }

            payDueTo.AmountToPay = currentPayDueTo.AmountToPay;
            payDueTo.PayToUserId = currentPayDueTo.PayToUserId;

            context.PayDueTos.Update(payDueTo);
        }

        await context.SaveChangesAsync();
        return payDueTos;
    }

    public async Task InitPayDueToAsync(Guid groupId, Guid userId)
    {
        var payDueTo = new PayDueTo
        {
            UserId = userId,
            GroupId = groupId
        };
        await context.PayDueTos.AddAsync(payDueTo);
        await context.SaveChangesAsync();
    }
}