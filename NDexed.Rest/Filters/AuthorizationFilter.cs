using System.Web;
using System.Web.Http.Filters;
using NDexed.Rest.Resources;
using NDexed.Rest._extensions;
using NDexed.Security;

namespace NDexed.Rest.Filters
{
    public class AuthorizationFilter : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            string tokenHeader = actionContext.Request.GetAuthToken();
            if (tokenHeader == null)
            {
                throw new HttpException(401, ErrorMessages.UserNotAuthorized);
            }

            var provider = (IAuthorizationTokenProvider)WebApiConfig.Container.GetService(typeof (IAuthorizationTokenProvider));
            provider.ValidateAuthorizationToken(tokenHeader);

            base.OnAuthorization(actionContext);
        }
    }
}