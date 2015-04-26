using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDexed.Messaging.Commands;

namespace NDexed.Messaging.Handlers
{
    public interface ICommandHandler<T> where T:ICommand
    {
        void Handle(T command);
    }
}
