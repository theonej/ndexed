using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NDexed.DataAccess.Repositories;
using NDexed.Domain;
using NDexed.Domain.Models.Profile;
using NDexed.Domain.Models.User;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Messages;
using NDexed.Messaging.Resources;
using NDexed.Security;

namespace NDexed.Messaging.Handlers
{
    public class UserCommandHandler : ICommandHandler<CreateUserCommand>,
                                      ICommandHandler<ResetPasswordCommand>,
                                      ICommandHandler<SetPasswordCommand>
    {
        private readonly IRepository<UserInfo, UserInfo> m_UserRepository;
        private readonly IHashProvider m_HashProvider;
        private readonly IMessager m_Messager;
        private readonly IAuthorizationTokenProvider m_TokenProvider;
        private readonly ISearchableRepository<UserInfo, UserInfo> m_SearchRepository;

        public UserCommandHandler(IHashProvider hashProvider,
                                  IRepository<UserInfo, UserInfo> userRepository,
                                  IMessager messager,
                                  IAuthorizationTokenProvider tokenProvider,
                                  ISearchableRepository<UserInfo, UserInfo> searchRepository)
        {
            Condition.Requires(tokenProvider).IsNotNull();
            Condition.Requires(userRepository).IsNotNull();
            Condition.Requires(hashProvider).IsNotNull();
            Condition.Requires(messager).IsNotNull();
            Condition.Requires(searchRepository).IsNotNull();

            m_UserRepository = userRepository;
            m_HashProvider = hashProvider;
            m_Messager = messager;
            m_TokenProvider = tokenProvider;
            m_SearchRepository = searchRepository;
        }

        public void Handle(CreateUserCommand command)
        {
            //this should be done in a handler
            UserInfo newUser = new UserInfo();
            newUser.CreatedDateTime = DateTime.UtcNow;
            newUser.EmailAddress = command.EmailAddress.ToLower();
            newUser.Id = command.Id;
            newUser.PasswordHash = GetUserPasswordHash(command.Password, newUser.Id);
            newUser.UserName = command.UserName;
            newUser.OrganizationId = command.OrganizationId;
            newUser.UserType = (UserType)command.UserType;

            m_UserRepository.Add(newUser);

            if (newUser.EmailAddress.Contains('@'))//guest users are just guids, so don't send them emails
            {
                //send email
                IMessageInfo message = GetRegistrationMessage(newUser);
                m_Messager.SendMessage(message);
            }
        }

        public void Handle(ResetPasswordCommand command)
        {
            UserInfo searchCriteria = new UserInfo();
            searchCriteria.EmailAddress = command.EmailAddress;

            UserInfo userData = m_SearchRepository.Search(searchCriteria).FirstOrDefault();
            if (userData == null)
            {
                throw new MissingMemberException(ErrorMessages.UserNotFoundByEmail);
            }

            userData.PasswordHash = GetUserPasswordHash(Guid.NewGuid().ToString(), userData.Id);
            m_UserRepository.Add(userData);

            //send email
            IMessageInfo message = GetResetPasswordMessage(userData, command.Source);
            m_Messager.SendMessage(message);
        }

        public void Handle(SetPasswordCommand command)
        {
            UserInfo query = new UserInfo();
            query.Id = command.UserId;

            UserInfo userData = m_UserRepository.Get(query);
            userData.PasswordHash = GetUserPasswordHash(command.Password, userData.Id);
            userData.UpdatedDateTime = DateTime.UtcNow;

            m_UserRepository.Add(userData);
        }

        #region Private Methods

        private string GetUserPasswordHash(string password, Guid salt)
        {
            string publicKey = ConfigurationManager.AppSettings["PublicKey"];

            string passwordHash = m_HashProvider.GenerateHash(password, salt.ToString(), publicKey);

            return passwordHash;
        }

        private IMessageInfo GetRegistrationMessage(UserInfo user)
        {
            EmailMessage message = new EmailMessage();
            message.Recipients = new List<string>() { user.EmailAddress };
            message.Sender = ConfigurationManager.AppSettings["SupportEmailAddress"];
            message.Title = "User Registration";
            message.UtcSent = DateTime.UtcNow;
            message.MessageId = Guid.NewGuid();
            message.Body = GetUserRegistrationEmailBody(user);

            return message;
        }

        private IMessageInfo GetResetPasswordMessage(UserInfo user, ResetSources source)
        {
            EmailMessage message = new EmailMessage();
            message.Recipients = new List<string>() { user.EmailAddress};
            message.Sender = ConfigurationManager.AppSettings["SupportEmailAddress"];
            message.Title = "Password Reset";
            message.UtcSent = DateTime.UtcNow;
            message.MessageId = Guid.NewGuid();
            message.Body = GetPasswordResetEmailBody(user, source);

            return message;
        }

        private string GetUserRegistrationEmailBody(UserInfo user)
        {
            string securityToken = m_TokenProvider.GenerateAuthorizationToken(user.Id);
            securityToken = HttpUtility.UrlEncode(securityToken);

            string setPasswordUrl = null;
            switch (user.UserType)
            {
                case(UserType.Admin):
                    setPasswordUrl = ConfigurationManager.AppSettings["AdminUserRegistrationUrl"];
                    break;
                default:
                    setPasswordUrl = ConfigurationManager.AppSettings["UserRegistrationUrl"];
                    break;
            }
            

            List<Tuple<string, string>> tagValues = new List<Tuple<string, string>>();
            tagValues.Add(new Tuple<string, string>("{{securityToken}}", securityToken));
            tagValues.Add(new Tuple<string, string>("{{setPasswordUrl}}", setPasswordUrl));

            string fileContents = Templates.UserRegistrationTemplate;
            foreach (Tuple<string, string> tagValue in tagValues)
            {
                fileContents = fileContents.Replace(tagValue.Item1, tagValue.Item2);
            }

            return fileContents;
        }

        private string GetPasswordResetEmailBody(UserInfo user, ResetSources source)
        {
            string securityToken = m_TokenProvider.GenerateAuthorizationToken(user.Id);
            securityToken = HttpUtility.UrlEncode(securityToken);

            string setPasswordUrl = ConfigurationManager.AppSettings["ClientSetPasswordUrl"];
            if (source == ResetSources.Admin)
            {
                setPasswordUrl = ConfigurationManager.AppSettings["AdminSetPasswordUrl"];
            }

            List<Tuple<string, string>> tagValues = new List<Tuple<string, string>>();
            tagValues.Add(new Tuple<string, string>("{{securityToken}}", securityToken));
            tagValues.Add(new Tuple<string, string>("{{setPasswordUrl}}", setPasswordUrl));

            string fileContents = Templates.ResetPasswordTemplate;
            foreach (Tuple<string, string> tagValue in tagValues)
            {
                fileContents = fileContents.Replace(tagValue.Item1, tagValue.Item2);
            }

            return fileContents;
        }

        #endregion


    }
}
