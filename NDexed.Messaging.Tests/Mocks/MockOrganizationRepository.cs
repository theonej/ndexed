
using System;
using System.Collections.Generic;
using System.Linq;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models;

namespace Waitless.Messaging.Tests.Mocks
{
    internal class MockOrganizationRepository : IRepository<Guid, Organization>
    {
        private List<Organization> m_Organizations = new List<Organization>();
        
        public Organization Get(Guid id)
        {
            return m_Organizations.FirstOrDefault(org => org.OrganizationId == id);
        }

        public Guid Add(Organization item)
        {
            m_Organizations.Add(item);

            return item.OrganizationId;
        }

        public void Remove(Organization item)
        {
            m_Organizations.Remove(item);
        }

        public System.Collections.Generic.IEnumerable<Organization> GetAll()
        {
            return m_Organizations;
        }
    }
}
