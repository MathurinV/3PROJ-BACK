using DAL.Models.PaymentDetails;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class PayDueToRepository(MoneyMinderDbContext context) : IPayDueToRepository
{
    public async Task<ICollection<PayDueTo>> RefreshPayDueTosAsync(Guid groupId)
    {
        var userBalances = new Dictionary<Guid, decimal>();
        var groupUsers = await context.UserGroups.Where(userGroup => userGroup.GroupId == groupId).ToListAsync();
        foreach (var currentUser in groupUsers) userBalances[currentUser.UserId] = currentUser.Balance;

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

        await ResetPayDueTosAsync(groupId);

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

    public async Task<PayDueTo> GetPayDueToAsync(Guid groupId, Guid payerId)
    {
        var payDueTo = await context.PayDueTos.FirstOrDefaultAsync(payDueTo =>
            payDueTo.GroupId == groupId && payDueTo.UserId == payerId);
        if (payDueTo == null) throw new Exception("PayDueTo not found");
        return payDueTo;
    }

    public async Task UpdateAsync(PayDueTo payDueTo)
    {
        context.PayDueTos.Update(payDueTo);
        await context.SaveChangesAsync();
    }

    public async Task ResetPayDueTosAsync(Guid groupId)
    {
        var payDueTos = await context.PayDueTos.Where(payDueTo => payDueTo.GroupId == groupId).ToListAsync();
        foreach (var payDueTo in payDueTos)
        {
            payDueTo.AmountToPay = 0;
            payDueTo.PayToUserId = null;
            context.PayDueTos.Update(payDueTo);
        }

        await context.SaveChangesAsync();
    }
}