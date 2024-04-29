using DAL.Models.UserGroups;
using DAL.Models.Users;

namespace DAL.Models.PaymentDetails;

public class PayDueTo
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public decimal? AmountToPay { get; set; }
    public Guid PayToUserId { get; set; }
    public AppUser PayToUser { get; set; } = null!;
    public UserGroup UserGroup { get; set; } = null!;
}