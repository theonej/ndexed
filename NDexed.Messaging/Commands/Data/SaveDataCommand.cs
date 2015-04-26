
using System;
using NDexed.Domain.Models.Data;

namespace NDexed.Messaging.Commands.Data
{
    public class SaveDataCommand : ICommand
    {
        public Guid Id { get; set; }
        public DataInfo Data { get; set; }
        public Guid UserId { get; set; }
    }
}
