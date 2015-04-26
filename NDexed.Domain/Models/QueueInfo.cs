using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Domain.Models
{
    public enum QueueStatus
    {
        Waiting = 0,
        Ready = 1,
        Serviced = 2
    }

    public class QueueInfo : AuditInfo
    {
        public Guid Id { get; set; }
        public Guid ServiceProviderId{get;set;}
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime Expiration{get;set;}
        public DateTime DateAdded { get; set; }
        public QueueStatus Status { get; set; }
        public int PartySize { get; set; }
        public string Requests { get; set; }
    }
}
