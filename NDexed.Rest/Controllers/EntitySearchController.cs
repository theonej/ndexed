using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CuttingEdge.Conditions;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.Data;
using NDexed.Rest.Filters;

namespace NDexed.Rest.Controllers
{
    public class DataSearchController : ApiController
    {
        private readonly IReadRepository<Guid, DataInfo> m_ReadRepository;

        public DataSearchController(IReadRepository<Guid, DataInfo> readRepository)
        {
            Condition.Requires(readRepository).IsNotNull();

            m_ReadRepository = readRepository;
        }

        [HttpGet]
        [AuthorizationFilter]
        [ExceptionFilter]
        public HttpResponseMessage Search(string query)
        {
            var searchCriteria = new DataInfo {SerializedData = query};

            var results = m_ReadRepository.Search(searchCriteria);

            var response = Request.CreateResponse(HttpStatusCode.OK, results);

            return response;
        }
    }
}
