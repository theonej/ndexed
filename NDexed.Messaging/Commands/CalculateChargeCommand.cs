using System;

namespace NDexed.Messaging.Commands
{
    public class CalculateChargeCommand : ICommand
    {
        public Guid Id { get; set; }
        public Guid ServiceProviderId { get; set; }
    }
}
