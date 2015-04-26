
using System;
using NDexed.Messaging.Commands;

namespace Waitless.Messaging.Commands
{
    public class CreateOrganizationCommand : ICommand 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public string WebsiteUrl { get; set; }
    }
}
