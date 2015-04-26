using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models;
using NDexed.Domain.Resources;
using NDexed.Messaging.Commands.Queues;

namespace NDexed.Messaging.Handlers
{
    public class QueueCommandHandler : ICommandHandler<AddQueueItemCommand>,
                                       ICommandHandler<UpdateQueueItemCommand>
    {
        private readonly IRepository<Guid, QueueInfo> m_QueueRepository;
        private readonly ISearchableRepository<Tuple<Guid, Guid>, QueueInfo> m_QueryRepository;

        public QueueCommandHandler(IRepository<Guid, QueueInfo> queueRepository,
                                   ISearchableRepository<Tuple<Guid, Guid>, QueueInfo> queryRepository)
        {
            Condition.Requires(queueRepository).IsNotNull();
            Condition.Requires(queryRepository).IsNotNull();

            m_QueueRepository = queueRepository;
            m_QueryRepository = queryRepository;
        }

        public void Handle(AddQueueItemCommand command)
        {
            Tuple<Guid, Guid> searchCriteria = new Tuple<Guid,Guid>(command.ServiceProviderId, command.UserId);
            QueueInfo existingQueueItem = m_QueryRepository.Search(searchCriteria).FirstOrDefault();
            if (existingQueueItem != null && existingQueueItem.Status != QueueStatus.Serviced)
            {
                throw new InvalidOperationException(ErrorMessages.AlreadyInQueue);
            }

            QueueInfo queueItem = new QueueInfo();
            queueItem.CreatedBy = command.UserId.ToString();
            queueItem.CreatedDateTime = DateTime.UtcNow;
            queueItem.DateAdded = DateTime.UtcNow;
            queueItem.Expiration = DateTime.UtcNow.AddHours(6);
            queueItem.Id = command.QueueItemId;
            queueItem.Name = command.Name;
            queueItem.PartySize = command.PartySize;
            queueItem.Requests = command.Requests;
            queueItem.ServiceProviderId = command.ServiceProviderId;
            queueItem.Status = QueueStatus.Waiting;
            queueItem.UserId = command.UserId;

            m_QueueRepository.Add(queueItem);
        }

        public void Handle(UpdateQueueItemCommand command)
        {
            QueueInfo queueItem = m_QueueRepository.Get(command.QueueItemId);
            queueItem.Status = command.Status;
            queueItem.Requests = command.Requests;
            queueItem.PartySize = command.PartySize;
            queueItem.UpdatedBy = command.UpdatedBy;
            queueItem.UpdatedDateTime = DateTime.UtcNow;

            m_QueueRepository.Add(queueItem);
        }
    }
}
