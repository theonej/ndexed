using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Messages
{
    public interface IMessager
    {
        void SendMessage(IMessageInfo message);
    }
}
