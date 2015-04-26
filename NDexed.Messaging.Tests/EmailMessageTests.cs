using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDexed.Messaging.Messages;
using System.Collections.Generic;
using System.Configuration;
using Waitless.Messaging.Tests.Mocks;

namespace NDexed.Messaging.Tests
{
    [TestClass]
    public class EmailMessageTests
    {
        [TestMethod]
        public void SendPasswordResetEmail()
        {
            var messager = new MockMailMessager();

            EmailMessage message = new EmailMessage();
            message.Recipients = new List<string>() { "technology@n-dexed.com" };
            message.Sender = ConfigurationManager.AppSettings["SupportEmailAddress"];
            message.Title = "Password Reset";
            message.UtcSent = DateTime.UtcNow;
            message.MessageId = Guid.NewGuid();
            message.Body = "securityToken";

            messager.SendMessage(message);
        }
    }
}
