﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Commands
{
    public interface ICommand
    {
        Guid Id { get; set; }
    }
}
