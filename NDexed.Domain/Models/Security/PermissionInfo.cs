using System;

namespace NDexed.Domain.Models.Security
{
    public class PermissionInfo : AuditInfo
    {
        public Guid PermissionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
