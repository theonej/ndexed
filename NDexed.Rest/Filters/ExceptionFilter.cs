using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Authentication;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace NDexed.Rest.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            Type exceptionType = context.Exception.GetType();
            if (exceptionType == typeof(HttpException))
            {
                HttpStatusCode code = (HttpStatusCode)((HttpException)context.Exception).GetHttpCode();
                context.Response = context.Request.CreateErrorResponse(code, context.Exception.Message);
            }
            else
            {
                context.Response = context.Request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, context.Exception.Message);

                Dictionary<Type, HttpStatusCode> codes = new Dictionary<Type, HttpStatusCode>();
                codes.Add(typeof(FormatException), HttpStatusCode.Unauthorized);
                codes.Add(typeof(SecurityException), HttpStatusCode.Unauthorized);
                codes.Add(typeof(AuthenticationException), HttpStatusCode.Unauthorized);
                codes.Add(typeof(MissingFieldException), HttpStatusCode.NotFound);
                codes.Add(typeof(HttpException), context.Response.StatusCode);

                if (codes.ContainsKey(exceptionType))
                {
                    context.Response = context.Request.CreateErrorResponse(codes[exceptionType], context.Exception.Message);
                }
            }
            base.OnException(context);
        }
    }
}