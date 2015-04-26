
using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace NDexed.Rest.Filters
{
    public class RouteActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var arguments = actionContext.ActionArguments;
            arguments["id"] = Guid.NewGuid();

            base.OnActionExecuting(actionContext);
        }
    }
}