using System;

namespace NDexed.Domain.Models.User
{
    public class UserApplicationInfo : AuditInfo  
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool BillingIsSetup { get; set; }
        public bool SecurityIsSetup { get; set; }
        public bool DataIsSetup { get; set; }
        public bool SearchIsSetup { get; set; }
        public bool AuditingIsSetup { get; set; }
        public bool MonitoringIsSetup { get; set; }
    }
}
