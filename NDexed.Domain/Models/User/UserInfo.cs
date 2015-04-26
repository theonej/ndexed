using System;
using System.Collections.Generic;
using NDexed.Domain.Models.Security;

namespace NDexed.Domain.Models.User
{
    public enum UserType
    {
        Customer = 0,
        Admin = 1
    }

    public class UserInfo : AuditInfo
    {
        public Guid Id { get; set; }
        public UserType UserType { get; set; }
        public Guid OrganizationId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public List<GroupInfo> Membership { get; set; }
        public CustomerInfo ExternalCustomerInfo { get; set; }
        public UserApplicationInfo ApplicationInfo { get; set; }
    }
}
