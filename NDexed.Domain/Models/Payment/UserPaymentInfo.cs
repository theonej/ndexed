using System;

namespace NDexed.Domain.Models.Payment
{
    public class UserPaymentInfo : AuditInfo
    {
        public Guid UserPaymentInfoId { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
    }
}
