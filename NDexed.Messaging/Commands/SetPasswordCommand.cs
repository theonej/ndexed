using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Commands
{
    public class SetPasswordCommand :ICommand
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }
    }
}
