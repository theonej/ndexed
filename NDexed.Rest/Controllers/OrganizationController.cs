using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CuttingEdge.Conditions;
using NDexed.Messaging.Handlers;
using NDexed.Rest.Filters;
using Waitless.Messaging.Commands;

namespace NDexed.Rest.Controllers
{
    public class OrganizationController : BaseController
    {
        private readonly ICommandHandler<CreateOrganizationCommand> m_CreateHandler;

        public OrganizationController(ICommandHandler<CreateOrganizationCommand> createHandler)
        {
            Condition.Requires(createHandler).IsNotNull();

            m_CreateHandler = createHandler;
        }

        [HttpPost]
        [ExceptionFilter]
        public HttpResponseMessage Post(CreateOrganizationCommand command)
        {
            if (command.Id == Guid.Empty)
            {
                command.Id = Guid.NewGuid();
            }

            m_CreateHandler.Handle(command);

            var response = Request.CreateResponse(HttpStatusCode.Created, command.Id);

            return response;
        }
    }
}
