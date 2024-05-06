using DAL.Models.PaymentDetails;
using DAL.Repositories;

namespace API.Repositories;

public class PayDueToRepository(MoneyMinderDbContext context): IPayDueToRepository
{
    public async Task<ICollection<PayDueTo>> RefreshAllPayDuesAsync(List<ICollection<KeyValuePair<Guid, decimal>>> addUserExpenseResults)
    {
        var payDueTos = new List<PayDueTo>();
        foreach (var userExpenseResults in addUserExpenseResults)
        {
            foreach (var userExpenseResult in userExpenseResults)
            {
                var payDueTo = new PayDueTo
                {
                    UserId = userExpenseResult.Key,
                    AmountToPay = userExpenseResult.Value
                };
                payDueTos.Add(payDueTo);
            }
        }

        await context.PayDueTos.AddRangeAsync(payDueTos);
        if (await context.SaveChangesAsync() == 0) throw new Exception("Failed to insert pay dues");
        return payDueTos;
    }
}