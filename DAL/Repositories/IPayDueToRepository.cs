using DAL.Models.PaymentDetails;

namespace DAL.Repositories;

public interface IPayDueToRepository
{
    Task<ICollection<PayDueTo>> RefreshPayDueTosAsync(Guid groupId);
    Task InitPayDueToAsync(Guid groupId, Guid userId);
}