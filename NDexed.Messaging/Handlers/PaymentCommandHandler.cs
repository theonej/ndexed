using System;
using System.Security.Principal;
using CuttingEdge.Conditions;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.Payment;
using NDexed.Domain.Models.User;
using NDexed.Domain.Resources;
using NDexed.Messaging.Commands;
using NDexed.Security;

namespace NDexed.Messaging.Handlers
{
    public class PaymentCommandHandler : ICommandHandler<SavePaymentInfoCommand>
    {
        private readonly IRepository<UserInfo, UserInfo> m_UserRepository;
        private readonly IRepository<UserPaymentInfo, UserPaymentInfo> m_UserPaymentRepository;
        private readonly IEncryptor m_Encryptor;

        public PaymentCommandHandler(IRepository<UserInfo, UserInfo> userRepository,
                                     IRepository<UserPaymentInfo, UserPaymentInfo> userPaymentRepository,
                                     IEncryptor encryptor)
        {
            Condition.Requires(userRepository).IsNotNull();
            Condition.Requires(userPaymentRepository).IsNotNull();
            Condition.Requires(encryptor).IsNotNull();

            m_UserRepository = userRepository;
            m_UserPaymentRepository = userPaymentRepository;
            m_Encryptor = encryptor;
        }

        public void Handle(SavePaymentInfoCommand command)
        {
            UserInfo user = GetUser(command.UserId);

            command.PaymentInfo.UserId = user.Id;
            command.PaymentInfo.CreatedBy = user.EmailAddress;
            command.PaymentInfo.CreatedDateTime = DateTime.UtcNow;
            command.PaymentInfo.Token = m_Encryptor.EncryptValue(command.PaymentInfo.Token);

            m_UserPaymentRepository.Add(command.PaymentInfo);

            if (user.ApplicationInfo == null)
            {
                user.ApplicationInfo = new UserApplicationInfo();
            }

            user.ApplicationInfo.BillingIsSetup = true;
            m_UserRepository.Add(user);
        }

        #region Private Methods

        private UserInfo GetUser(Guid userId)
        {
            var searchCriteria = new UserInfo
            {
                Id = userId
            };
            UserInfo userData = m_UserRepository.Get(searchCriteria);
            if (userData == null)
            {
                throw new InvalidOperationException(ErrorMessages.UserNotFound);
            }

            return userData;
        }

        #endregion
    }
}
