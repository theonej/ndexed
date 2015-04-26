using System;
using System.Linq;
using CuttingEdge.Conditions;
using NDexed.DataAccess.Repositories;
using NDexed.Domain;
using NDexed.Domain.Models;
using NDexed.Domain.Models.User;
using NDexed.Domain.Resources;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using Waitless.Messaging.Commands;

namespace Waitless.Messaging.Handlers
{
    public class OrganizationCommandHandler : ICommandHandler<CreateOrganizationCommand>
    {
        private readonly IRepository<Guid, Organization> m_OrganizationRepository;
        private readonly ISearchableRepository<UserInfo, UserInfo> m_UserRepository;
        private readonly ICommandHandler<CreateUserCommand> m_CreateHandler;

        public OrganizationCommandHandler(IRepository<Guid, Organization> organizationRepository,
                                          ISearchableRepository<UserInfo, UserInfo> userRepository,
                                          ICommandHandler<CreateUserCommand> createHandler)
        {
            Condition.Requires(organizationRepository).IsNotNull();
            Condition.Requires(userRepository).IsNotNull();
            Condition.Requires(createHandler).IsNotNull();

            m_OrganizationRepository = organizationRepository;
            m_UserRepository = userRepository;
            m_CreateHandler = createHandler;
        }

        public void Handle(CreateOrganizationCommand command)
        {
            var searchCriteria = new UserInfo();
            searchCriteria.EmailAddress = command.PrimaryEmailAddress;
            var existingUser = m_UserRepository.Search(searchCriteria).FirstOrDefault();
            if (existingUser != null)
            {
                throw new ArgumentException(string.Format(ErrorMessages.UserEmailInUse, command.PrimaryEmailAddress));
            }

            var organization = new Organization
            {
                CreatedBy = "Service",
                CreatedDateTime = DateTime.UtcNow,
                Name = command.Name,
                OrganizationId = command.Id,
                PrimaryEmailAddress = command.PrimaryEmailAddress,
                WebsiteUrl = command.WebsiteUrl
            };

            m_OrganizationRepository.Add(organization);

            var userCommand = new CreateUserCommand
            {
                EmailAddress = command.PrimaryEmailAddress,
                Id = Guid.NewGuid(),
                OrganizationId = organization.OrganizationId,
                Password = Guid.NewGuid().ToString(),
                UserName = command.PrimaryEmailAddress,
                UserType = 0
            };

            m_CreateHandler.Handle(userCommand);
        }
    }
}
