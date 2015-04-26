using System.Collections.Generic;
using System.Linq;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.User;

namespace Waitless.Messaging.Tests.Mocks
{
    class MockUserRepository : IRepository<UserInfo, UserInfo>, ISearchableRepository<UserInfo, UserInfo>
    {
        private List<UserInfo> m_Users = new List<UserInfo>();
 
        public UserInfo Get(UserInfo id)
        {
            return m_Users.FirstOrDefault(user => user.Id == id.Id);
        }

        public UserInfo Add(UserInfo item)
        {
            m_Users.Add(item);
            return item;
        }

        public void Remove(UserInfo item)
        {
            m_Users.Remove(item);
        }

        public System.Collections.Generic.IEnumerable<UserInfo> GetAll()
        {
            return m_Users;
        }

        public System.Collections.Generic.IEnumerable<UserInfo> Search(UserInfo criteria)
        {
            return m_Users.Where(user => user.EmailAddress == criteria.EmailAddress);
        }
    }
}
