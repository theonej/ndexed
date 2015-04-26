using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Commands
{
    public class CreateUserCommand : ICommand
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public int UserType { get; set; }
    }
}
