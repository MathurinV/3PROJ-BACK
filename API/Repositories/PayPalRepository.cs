using DAL.Models.Users;
using DAL.Repositories;
using PayPal.Api;

namespace API.Repositories;

public class PayPalRepository : IPayPalRepository
{
    private readonly APIContext _apiContext;

    public PayPalRepository()
    {
        var paypalConfig = new Dictionary<string, string>
        {
            { "mode", "sandbox" },
            { "clientId", DockerEnv.PaypalClientId },
            { "clientSecret", DockerEnv.PaypalClientSecret }
        };
        var accessToken = new OAuthTokenCredential(paypalConfig).GetAccessToken();
        _apiContext = new APIContext(accessToken);
    }

    public Payment Test()
    {
        var payment = Payment.Create(_apiContext, new Payment
        {
            intent = "sale",
            payer = new Payer
            {
                payment_method = "paypal"
            },
            transactions = new List<Transaction>
            {
                new()
                {
                    description = "Transaction description.",
                    invoice_number = "001",
                    amount = new Amount
                    {
                        currency = "USD",
                        total = "100.00",
                        details = new Details
                        {
                            tax = "15",
                            shipping = "10",
                            subtotal = "75"
                        }
                    },
                    item_list = new ItemList
                    {
                        items = new List<Item>
                        {
                            new()
                            {
                                name = "Item Name",
                                currency = "USD",
                                price = "15",
                                quantity = "5",
                                sku = "sku"
                            }
                        }
                    }
                }
            },
            redirect_urls = new RedirectUrls
            {
                return_url = "http://mysite.com/return",
                cancel_url = "http://mysite.com/cancel"
            }
        });
        return payment;
    }

    public Payment CreatePaymentBetweenUsers(AppUser payer, AppUser payee, decimal amount)
    {
        var payerEmail = payer.Email ?? throw new Exception("Payer email is null");
        var payeeEmail = payee.Email ?? throw new Exception("Payee email is null");

        var payment = new Payment
        {
            intent = "sale",
            payer = new Payer
            {
                payment_method = "paypal",
                payer_info = new PayerInfo { email = payerEmail }
            },
            transactions = new List<Transaction>
            {
                new()
                {
                    amount = new Amount
                    {
                        currency = "EUR",
                        total = amount.ToString("F2")
                    },
                    payee = new Payee
                    {
                        email = payeeEmail
                    },
                    description = $"Moneyminder payment from {payer.UserName} to {payee.UserName}"
                }
            },
            redirect_urls = new RedirectUrls
            {
                return_url = "http://example.com/return",
                cancel_url = "http://example.com/cancel"
            }
        };

        var createdPayment = payment.Create(_apiContext);

        return createdPayment;
    }
}