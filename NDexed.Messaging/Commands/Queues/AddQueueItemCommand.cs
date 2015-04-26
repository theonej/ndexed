using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Commands.Queues
{
    public class AddQueueItemCommand : ICommand
    {
        public Guid Id { get; set; }
        public Guid QueueItemId { get; set; }
        public Guid ServiceProviderId { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public int PartySize { get; set; }
        public string Requests { get; set; }
    }
}
