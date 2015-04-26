using System;

namespace NDexed.Domain.Models
{
    public abstract class AuditInfo
    {
        public DateTime CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
    }
}
