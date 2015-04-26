
using System;
using NDexed.Domain.Models.Payment;

namespace NDexed.Messaging.Commands
{
    public class SavePaymentInfoCommand : ICommand
    {
        public Guid Id { get; set; }
        public UserPaymentInfo PaymentInfo { get; set; }
        public Guid UserId { get; set; }
    }
}
