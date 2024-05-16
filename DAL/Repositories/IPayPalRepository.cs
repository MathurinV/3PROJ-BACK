using DAL.Models.Users;
using PayPal.Api;

namespace DAL.Repositories;

public interface IPayPalRepository
{
    Payment Test();
    Payment CreatePaymentBetweenUsers(AppUser payer, AppUser payee, decimal amount);
}