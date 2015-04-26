using System;

namespace NDexed.Payments
{
    public class PaymentCardInfo
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string CVC { get; set; }
        public string LastFourDigits { get; set; }
        public string CardToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
