using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Messages
{
    public interface IMessageInfo
    {
        Guid MessageId { get; set; }
        string Title { get; set; }
        string Body { get; set; }
        List<String> Recipients { get; set; }
        string Sender { get; set; }
        DateTime UtcSent { get; set; }
    }
}
