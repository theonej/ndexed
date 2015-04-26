using System;

namespace NDexed.Messaging.Commands
{
    public enum ResetSources
    {
        Client = 0,
        Admin = 1
    }

    public class ResetPasswordCommand : ICommand
    {
        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public ResetSources Source { get; set; }
    }
}
