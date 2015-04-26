using System.Collections.Generic;
using System.Linq;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.Payment;

namespace Waitless.Messaging.Tests.Mocks
{
    public class MockUserPaymentRepository : IRepository<UserPaymentInfo, UserPaymentInfo>
    {
        private List<UserPaymentInfo> m_UserPayments = new List<UserPaymentInfo>();
 
        public UserPaymentInfo Get(UserPaymentInfo id)
        {
            return m_UserPayments.FirstOrDefault(payment => payment.UserPaymentInfoId == id.UserPaymentInfoId);
        }

        public UserPaymentInfo Add(UserPaymentInfo item)
        {
            m_UserPayments.Add(item);

            return item;
        }

        public void Remove(UserPaymentInfo item)
        {
            m_UserPayments.Remove(item);
        }

        public System.Collections.Generic.IEnumerable<UserPaymentInfo> GetAll()
        {
            return m_UserPayments;
        }
    }
}
