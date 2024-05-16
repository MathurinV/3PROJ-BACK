using DAL.Models.PaymentDetails;

namespace DAL.Repositories;

public interface IPayDueToRepository
{
    Task<ICollection<PayDueTo>> RefreshPayDueTosAsync(Guid groupId);
    Task InitPayDueToAsync(Guid groupId, Guid userId);
    Task<PayDueTo> GetPayDueToAsync(Guid groupId, Guid payerId);
    Task UpdateAsync(PayDueTo payDueTo);
}