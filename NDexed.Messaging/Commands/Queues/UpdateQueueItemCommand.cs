using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDexed.Domain.Models;

namespace NDexed.Messaging.Commands.Queues
{
    public class UpdateQueueItemCommand : ICommand
    {
        public Guid Id { get; set; }
        public Guid QueueItemId { get; set; }
        public QueueStatus Status { get; set; }
        public int PartySize { get; set; }
        public string Requests { get; set; }
        public string UpdatedBy { get; set; }
    }
}
