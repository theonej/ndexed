using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using NDexed.Rest.Resources;
using NDexed.Security;

namespace NDexed.Rest._extensions
{
    internal static class HttpRequestMessageExtensions
    {
        internal const string AUTH_TOKEN_NAME = "NDexedAuthToken";

        internal static string GetAuthToken(this HttpRequestMessage request)
        {
            try
            {
                string tokenHeader = request.Headers
                               .FirstOrDefault(header => header.Key == AUTH_TOKEN_NAME)
                               .Value.FirstOrDefault();

                if (string.IsNullOrEmpty(tokenHeader))
                {
                    throw new HttpException(401, ErrorMessages.UserNotAuthorized);
                }
                return tokenHeader;
            }
            catch (ArgumentNullException)
            {
                throw new HttpException(401, ErrorMessages.UserNotAuthorized);
            }
        }

        internal static Guid GetUserId(this HttpRequestMessage request)
        {
            try
            {
                string tokenHeader = request.Headers
                    .FirstOrDefault(header => header.Key == AUTH_TOKEN_NAME)
                    .Value.FirstOrDefault();

                if (string.IsNullOrEmpty(tokenHeader))
                {
                    throw new HttpException(401, ErrorMessages.UserNotAuthorized);
                }

                IAuthorizationTokenProvider tokenProvider = (IAuthorizationTokenProvider)WebApiConfig.Container.GetService(typeof(IAuthorizationTokenProvider));
                Guid userId = tokenProvider.ValidateAuthorizationToken(tokenHeader);

                return userId;
            }
            catch (Exception)
            {
                throw new HttpException(401, ErrorMessages.UserNotAuthorized);
            }
        }
    }
}