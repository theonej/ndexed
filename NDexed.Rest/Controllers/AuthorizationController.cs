using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Web.Http;
using CuttingEdge.Conditions;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.User;
using NDexed.Domain.Resources;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using NDexed.Security;

namespace NDexed.Rest.Controllers
{
    public class AuthorizationController : BaseController
    {
        const string BASIC_AUTH_SCHEME = "basic";

        private readonly IAuthorizationTokenProvider m_TokenProvider;
        private readonly ICommandHandler<CreateUserCommand> m_UserCommandHandler;
        private readonly ISearchableRepository<UserInfo, UserInfo> m_SearchRepository;
        private readonly IHashProvider m_HashProvider;

        public AuthorizationController(IAuthorizationTokenProvider tokenProvider,
                                       ICommandHandler<CreateUserCommand> userCommandHandler,
                                       ISearchableRepository<UserInfo, UserInfo> searchRepository,
                                       IHashProvider hashProvider,
                                       IRepository<UserInfo, UserInfo> userRepository)
        {
            Condition.Requires(tokenProvider).IsNotNull();
            Condition.Requires(userCommandHandler).IsNotNull();
            Condition.Requires(searchRepository).IsNotNull();
            Condition.Requires(hashProvider).IsNotNull();
            Condition.Requires(userRepository).IsNotNull();

            m_TokenProvider = tokenProvider;
            m_UserCommandHandler = userCommandHandler;
            m_SearchRepository = searchRepository;
            m_HashProvider = hashProvider;
        }

        [HttpGet]
        public HttpResponseMessage Get(string emailAddress)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Accepted);

            try
            {
                UserInfo searchCriteria = new UserInfo();
                searchCriteria.EmailAddress = emailAddress;

                UserInfo userData = m_SearchRepository.Search(searchCriteria).FirstOrDefault();
                if (userData != null)
                {
                    throw new InvalidOperationException(ErrorMessages.UserEmailInUse);
                }

                response = Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (InvalidOperationException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return response;
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Accepted);

            try
            {
                //get the basic auth credentials
                string basicAuthHeader = GetBasicAuthValue(Request.Headers.Authorization);

                //find a user that matches
                UserInfo authenticatedUser = GetUser(basicAuthHeader);

                //create security token
                string securityToken = m_TokenProvider.GenerateAuthorizationToken(authenticatedUser.Id);

                var content = new StringContent(securityToken);
                content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = content;
            }
            catch (AuthenticationException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (SecurityException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return response;
        }

        

        [HttpPost]
        public HttpResponseMessage Post(CreateUserCommand command)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Accepted);

            try
            {
                if (command.Id == Guid.Empty)
                    command.Id = Guid.NewGuid();

                UserInfo searchCriteria = new UserInfo();
                searchCriteria.EmailAddress = command.EmailAddress;

                UserInfo userData = m_SearchRepository.Search(searchCriteria).FirstOrDefault();
                if (userData != null)
                {
                    throw new InvalidOperationException(ErrorMessages.UserEmailInUse);
                }

                m_UserCommandHandler.Handle(command);

                response = Request.CreateResponse(HttpStatusCode.Created, command.Id);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            }
            catch (InvalidOperationException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return response;
        }

        #region Private Methods

        private string GetBasicAuthValue(AuthenticationHeaderValue value)
        {
            string providedScheme = value.Scheme.ToLower();
            if (providedScheme != BASIC_AUTH_SCHEME)
            {
                string errorMessage = string.Format(ErrorMessages.AuthenticationSchemeNotSupported, providedScheme);
                throw new NotSupportedException(errorMessage);
            }

           byte[] valueBytes = Convert.FromBase64String(value.Parameter);

           string returnValue = Encoding.UTF8.GetString(valueBytes);

           return returnValue;
        }

        private UserInfo GetUser(string basicAuthHeader)
        {
            string[] authParts = basicAuthHeader.Split(':');
            if (authParts.Length != 2)
            {
                throw new FormatException(ErrorMessages.CredentialFormatError);
            }

            UserInfo searchCriteria = new UserInfo();
            searchCriteria.EmailAddress = authParts[0];

            UserInfo userData = m_SearchRepository.Search(searchCriteria).FirstOrDefault();
            if (userData == null)
            {
                throw new AuthenticationException(ErrorMessages.UserNotFound);
            }

            string passwordHash = GetUserPasswordHash(authParts[1], userData.Id);
            if (userData.PasswordHash != passwordHash)
            {
                throw new SecurityException(ErrorMessages.UserNotFound);
            }

            return userData;
        }

        private string GetUserPasswordHash(string password, Guid salt)
        {
            string publicKey = ConfigurationManager.AppSettings["PublicKey"];

            string passwordHash = m_HashProvider.GenerateHash(password, salt.ToString(), publicKey);

            return passwordHash;
        }

        #endregion
    }
}
