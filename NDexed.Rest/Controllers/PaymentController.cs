using System;
using System.Net;
using CuttingEdge.Conditions;
using System.Net.Http;
using System.Web.Http;
using NDexed.Domain.Resources;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using NDexed.Payments;
using NDexed.Rest.Filters;
using NDexed.Rest._extensions;

namespace NDexed.Rest.Controllers
{
    public class PaymentController : ApiController
    {
        private readonly ICommandHandler<SavePaymentInfoCommand> m_SaveHandler;

        public PaymentController(ICommandHandler<SavePaymentInfoCommand> saveHandler)
        {
            Condition.Requires(saveHandler).IsNotNull();
            
            m_SaveHandler = saveHandler;
        }

        [HttpPost]
        [AuthorizationFilter]
        [ExceptionFilter]
        public HttpResponseMessage Post(SavePaymentInfoCommand command)
        {
            if (command.PaymentInfo == null)
            {
                throw new ArgumentNullException(string.Format(ErrorMessages.MissingRequiredArgumentValue, "command.paymentInfo"));
            }

            command.UserId = Request.GetUserId();
            if (command.PaymentInfo.UserPaymentInfoId == Guid.Empty)
            {
                command.PaymentInfo.UserPaymentInfoId = Guid.NewGuid();
            }

            m_SaveHandler.Handle(command);

            var response = Request.CreateResponse(HttpStatusCode.Created, command.PaymentInfo.UserPaymentInfoId);

            return response;
        }
    }
}
