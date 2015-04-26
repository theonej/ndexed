using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Messages
{
    public class EmailMessage : IMessageInfo
    {
        public string Body { get; set; }
        public Guid MessageId { get; set; }
        public List<string> Recipients { get; set; }
        public string Sender { get; set; }
        public string Title { get; set; }
        public System.DateTime UtcSent { get; set; }
    }
}
