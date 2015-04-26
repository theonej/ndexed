using System;
using System.Collections.Generic;
using NDexed.Domain.Models;

namespace NDexed.Payments
{
    public class PaymentInfo
    {
        public Guid Id { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal SalesTax { get; set; }
    }
}
