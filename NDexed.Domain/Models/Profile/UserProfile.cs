using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NDexed.Domain.Models.Profile
{
    public class UserProfile : AuditInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public List<Group> Membership { get; set; }
    }
}
