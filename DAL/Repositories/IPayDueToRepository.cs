using DAL.Models.PaymentDetails;

namespace DAL.Repositories;

public interface IPayDueToRepository
{
    public Task<ICollection<PayDueTo>> RefreshAllPayDuesAsync(
        List<ICollection<KeyValuePair<Guid, decimal>>> addUserExpenseResults);
}