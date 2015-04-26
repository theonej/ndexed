
using System;
using System.Collections.Generic;

namespace NDexed.Domain.Models.Security
{
    public class GroupInfo: AuditInfo
    {
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public List<PermissionInfo> Permissions { get; set; } 
    }
}
