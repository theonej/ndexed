using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CuttingEdge.Conditions;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.Data;
using NDexed.Rest.Filters;
using NDexed.Rest._extensions;
using NDexed.Security;

namespace NDexed.Rest.Controllers
{
    public class DataController : BaseController
    {
        private readonly IRepository<Guid, DataInfo> m_DataRepository;
        private readonly IReadRepository<Guid, DataInfo> m_ReadRepository; 
        private readonly IEncryptor m_Encryptor;

        public DataController(IRepository<Guid, DataInfo> dataRepository,
                                IReadRepository<Guid, DataInfo> readRepository,
                                IEncryptor encryptor)
        {
            Condition.Requires(dataRepository).IsNotNull();
            Condition.Requires(readRepository).IsNotNull();
            Condition.Requires(encryptor).IsNotNull();

            m_DataRepository = dataRepository;
            m_ReadRepository = readRepository;
            m_Encryptor = encryptor;
        }

        [HttpGet]
        [AuthorizationFilter]
        [ExceptionFilter]
        public HttpResponseMessage Get(Guid id)
        {
            var data = m_DataRepository.Get(id);
            data.SerializedData = m_Encryptor.DecryptValue(data.SerializedData);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPut]
        [AuthorizationFilter]
        [ExceptionFilter]
        public HttpResponseMessage Save(DataInfo data)
        {
            if (data.DataId == Guid.Empty)
            {
                data.DataId = Guid.NewGuid();
            }
            if (data.CreatedBy == null)
            {
                data.CreatedBy = Request.GetUserId().ToString();
                data.CreatedDateTime = DateTime.UtcNow;
            }
            else
            {
                data.UpdatedBy = Request.GetUserId().ToString();
                data.UpdatedDateTime = DateTime.UtcNow;
            }

            m_ReadRepository.Add(data);

            data.SerializedData = m_Encryptor.EncryptValue(data.SerializedData);

            m_DataRepository.Add(data);

            var response = Request.CreateResponse(HttpStatusCode.OK, data.DataId);

            return response;
        }
    }
}
