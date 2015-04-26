using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NDexed.Messaging.Messages
{
    public class EmailMessenger : IMessager
    {
        public void SendMessage(IMessageInfo message)
        {
            MailMessage mailMessage = GetMailMessage(message);

            SmtpClient client = InitializeClient();
            client.Send(mailMessage);
        }

        #region Private Methods

        private static SmtpClient InitializeClient()
        {
            SmtpClient client = new SmtpClient();

            return client;
        }

        private static MailMessage GetMailMessage(IMessageInfo message)
        {
            MailMessage mailMessage = new MailMessage
            {
                Body = message.Body,
                From = new MailAddress(message.Sender),
                IsBodyHtml = true
            };

            mailMessage.Sender = mailMessage.From;
            mailMessage.Subject = message.Title;
            foreach (string recipient in message.Recipients)
            {
                mailMessage.To.Add(new MailAddress(recipient));
            }

            return mailMessage;
        }

        #endregion
    }
}
