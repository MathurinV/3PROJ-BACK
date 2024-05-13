using DAL.Models.PaymentDetails;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class PayDueToRepository(MoneyMinderDbContext context) : IPayDueToRepository
{
    public async Task<ICollection<PayDueTo>> RefreshPayDueTosAsync(Guid groupId)
    {
        Dictionary<Guid, decimal> userBalances = new Dictionary<Guid, decimal>();
        var groupUserExpense = await context.UserExpenses.Include(userExpense => userExpense.Expense)
            .Where(userExpense => userExpense.Expense.GroupId == groupId).ToListAsync();
        foreach (var currentUserExpense in groupUserExpense)
        {
            var currentUserBalance =
                userBalances.FirstOrDefault(userBalance => userBalance.Key == currentUserExpense.UserId);
            if (userBalances.ContainsKey(currentUserExpense.UserId))
            {
                userBalances[currentUserExpense.UserId] += currentUserExpense.Amount;
            }
            else
            {
                userBalances.Add(currentUserExpense.UserId, currentUserExpense.Amount);
            }
        }

        var userBalancesList = userBalances.OrderByDescending(pair => pair.Value).ToList();

        var negativeBalancesList = userBalancesList.Where(pair => pair.Value < 0).ToList();
        var positiveBalancesList = userBalancesList.Where(pair => pair.Value > 0).ToList();

        var payDueTos = new List<PayDueTo>();
        foreach (var currentBalance in negativeBalancesList)
        {
            payDueTos.Add(new PayDueTo
            {
                UserId = currentBalance.Key,
                AmountToPay = currentBalance.Value,
                GroupId = groupId,
                PayToUserId = positiveBalancesList.First().Key
            });
        }

        var positiveBalancesListCount = positiveBalancesList.Count;
        for (int i = 0; i < positiveBalancesListCount - 1; i++)
        {
            var newBalance = payDueTos.Where(to => to.PayToUserId == positiveBalancesList[i].Key)
                                 .Sum(to => to.AmountToPay) +
                             positiveBalancesList[i].Value;
            if (newBalance > 0)
            {
                payDueTos.Add(new PayDueTo
                {
                    UserId = positiveBalancesList[i].Key,
                    AmountToPay = newBalance,
                    GroupId = groupId,
                    PayToUserId = positiveBalancesList[i + 1].Key
                });
            }
            else break;
        }

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