using System;

namespace NDexed.Domain.Models.Payment
{
    public class PaymentConfirmationInfo
    {
        public Guid Id { get; set; }
        public string ConfirmationToken { get; set; }
        public string Status { get; set; }
    }
}
