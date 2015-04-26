using System;

namespace NDexed.Domain.Models.Data
{
    public class DataInfo :AuditInfo
    {
        public Guid DataId { get; set; }
        public Guid TypeId { get; set; }
        public Guid OrganizationId { get; set; }
        public string SerializedData { get; set; } 
    }
}
