using System;

namespace NDexed.Domain.Models
{
    public class Organization : AuditInfo
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public string WebsiteUrl { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public Guid? ParentId { get; set; }
    }
}
