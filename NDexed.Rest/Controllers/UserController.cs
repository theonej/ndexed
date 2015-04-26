using CuttingEdge.Conditions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.User;
using NDexed.Rest.Filters;
using NDexed.Security;
using NDexed.Rest._extensions;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using NDexed.Rest.Resources;
using System.Security.Authentication;

namespace NDexed.Rest.Controllers
{
    public class UserController : ApiController
    {
        private readonly IAuthorizationTokenProvider m_TokenProvider;
        private readonly IRepository<UserInfo, UserInfo> m_UserRepository;
        private readonly ICommandHandler<ResetPasswordCommand> m_CommandHandler;
        private readonly ISearchableRepository<UserInfo, UserInfo> m_SearchRepository;
        private readonly ICommandHandler<SetPasswordCommand> m_SetPasswordHandler;

        public UserController(IAuthorizationTokenProvider tokenProvider,
                              IRepository<UserInfo, UserInfo> userRepository,
                              ISearchableRepository<UserInfo, UserInfo> searchRepository,
                              ICommandHandler<ResetPasswordCommand> commandHandler,
                              ICommandHandler<SetPasswordCommand> setPasswordHandler)
        {
            Condition.Requires(tokenProvider).IsNotNull();
            Condition.Requires(userRepository).IsNotNull();
            Condition.Requires(commandHandler).IsNotNull();
            Condition.Requires(searchRepository).IsNotNull();
            Condition.Requires(setPasswordHandler).IsNotNull();

            m_TokenProvider = tokenProvider;
            m_UserRepository = userRepository;
            m_CommandHandler = commandHandler;
            m_SearchRepository = searchRepository;
            m_SetPasswordHandler = setPasswordHandler;
        }

        [HttpGet]
        [AuthorizationFilter]
        [ActionName("Get")]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                string authToken = Request.GetAuthToken();

                Guid userId = m_TokenProvider.ValidateAuthorizationToken(authToken);

                UserInfo searchCriteria = new UserInfo
                {
                    Id = userId
                };
                UserInfo userData = m_UserRepository.Get(searchCriteria);
                userData.PasswordHash = null;

                if (userData.ApplicationInfo == null)
                {
                    userData.ApplicationInfo = new UserApplicationInfo();
                }
                response = Request.CreateResponse(HttpStatusCode.OK, userData);
            }
            catch (AuthenticationException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (FormatException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ErrorMessages.InvalidToken);
            }
           
            return response;
        }

        [HttpGet]
        [AuthorizationFilter]
        [ActionName("GetById")]
        public HttpResponseMessage Get(Guid id)
        {
            
            UserInfo searchCriteria = new UserInfo
            {
                Id = id
            };
            UserInfo userData = m_UserRepository.Get(searchCriteria);
            userData.PasswordHash = null;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, userData);

            return response;
        }

        [HttpPost]
        [ActionName("ResetPassword")]
        public HttpResponseMessage Post(ResetPasswordCommand command)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, command.EmailAddress);

            UserInfo searchCriteria = new UserInfo();
            searchCriteria.EmailAddress = command.EmailAddress;

            UserInfo userData = m_SearchRepository.Search(searchCriteria).FirstOrDefault();
            if (userData == null)
            {
                throw new InvalidOperationException(ErrorMessages.UserNotFound);
            }

            m_CommandHandler.Handle(command);

            return response;
        }

        [HttpPost]
        [ActionName("SetPassword")]
        [AuthorizationFilter]
        public HttpResponseMessage Post(SetPasswordCommand command)
        {
            string authToken = Request.GetAuthToken();

            command.UserId = m_TokenProvider.ValidateAuthorizationToken(authToken);

            m_SetPasswordHandler.Handle(command);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Accepted, command.UserId);

            return response;
        }
    }
}
