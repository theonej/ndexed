
using System;
using CuttingEdge.Conditions;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.Data;
using NDexed.Messaging.Commands.Data;

namespace NDexed.Messaging.Handlers
{
    public class DataCommandHandler : ICommandHandler<SaveDataCommand>
    {
        private readonly IRepository<DataInfo, DataInfo> m_Repository;

        public DataCommandHandler(IRepository<DataInfo, DataInfo> repository)
        {
            Condition.Requires(repository).IsNotNull();

            m_Repository = repository;
        }

        public void Handle(SaveDataCommand command)
        {
            var data = command.Data;
            if (data.CreatedBy == null)
            {
                data.CreatedBy = command.UserId.ToString();
                data.CreatedDateTime = DateTime.UtcNow;
            }
            else
            {
                data.UpdatedBy = command.UserId.ToString();
                data.UpdatedDateTime = DateTime.UtcNow;
            }

            m_Repository.Add(data);
        }
    }
}
