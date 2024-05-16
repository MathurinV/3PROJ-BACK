using DAL.Models.UserGroups;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.PaymentDetails;

public class PayDueTo
{
    [GraphQLIgnore] public Guid UserId { get; set; }
    [GraphQLIgnore] public Guid GroupId { get; set; }
    public decimal AmountToPay { get; set; } = decimal.Zero;
    [GraphQLIgnore] public Guid? PayToUserId { get; set; } = null;
    public AppUser? PayToUser { get; set; }
    public UserGroup UserGroup { get; set; }
}