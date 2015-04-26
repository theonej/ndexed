using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDexed.AWS.Repository;
using NDexed.Domain.Models.Payment;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using NDexed.Security.Encryptors;
using Waitless.Messaging.Tests.Mocks;

namespace Waitless.Messaging.Tests
{
    [TestClass]
    public class PaymentCommandHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateNewUserPaymentThenFindThenDelete()
        {
            var command = new SavePaymentInfoCommand();
            command.UserId = Guid.Parse("c9276b76-c56a-43ba-96b5-83b48a5fcce3");
            command.Id = Guid.NewGuid();
            command.PaymentInfo = new UserPaymentInfo();
            command.PaymentInfo.Token = "thisisnotalegittoken";
            command.PaymentInfo.UserPaymentInfoId = Guid.NewGuid();

            var userRepo = new MockUserRepository();
            var paymentRepo = new MockUserPaymentRepository();
            var encryptor = new RijndaelManagedEncryptor();

            var handler = new PaymentCommandHandler(userRepo, paymentRepo, encryptor);
            handler.Handle(command);

                var paymentInfo = paymentRepo.Get(command.PaymentInfo);
            try
            {
                Assert.IsNotNull(paymentInfo);
                Assert.AreNotEqual(command.PaymentInfo.Token, "thisisnotalegittoken");
            }
            finally
            {
                paymentRepo.Remove(paymentInfo);
            }
        }
    }
}
