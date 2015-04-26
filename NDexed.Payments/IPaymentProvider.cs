
using NDexed.Domain;
using NDexed.Domain.Models.Payment;
using NDexed.Domain.Models.User;

namespace NDexed.Payments
{
    public interface IPaymentProvider
    {
        CustomerInfo CreateCustomer(UserInfo userData);
        PaymentCardInfo AddPaymentCard(CustomerInfo customer, PaymentCardInfo paymentCard);
        PaymentConfirmationInfo SubmitPayment(PaymentCardInfo paymentCard, PaymentInfo payment);
        string GenerateCardToken(PaymentCardInfo paymentCard);
    }
}
