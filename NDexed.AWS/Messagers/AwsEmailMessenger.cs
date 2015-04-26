using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDexed.Messaging.Messages;

namespace NDexed.AWS.Messagers
{
    public class AwsEmailMessenger : IMessager
    {
        public void SendMessage(IMessageInfo message)
        {
            AmazonSimpleEmailServiceClient client = InitializeClient();
            using (client)
            {
                SendEmailRequest request = CreateSendRequest(message);
                client.SendEmail(request);
            }
        }

        #region Private Methods

        private SendEmailRequest CreateSendRequest(IMessageInfo message)
        {
            SendEmailRequest request = new SendEmailRequest();
            request.Destination = new Destination(message.Recipients);

            request.Message = new Message
            {
                Subject = new Content(message.Title)
            };

            request.Message.Body = new Body();
            request.Message.Body.Html = new Content(message.Body);
            request.ReplyToAddresses = new List<string>() { message.Sender };
            request.Source = message.Sender;

            return request;
        }

        private AmazonSimpleEmailServiceClient InitializeClient()
        {
            AmazonSimpleEmailServiceConfig config = new AmazonSimpleEmailServiceConfig();
            config.RegionEndpoint = RegionEndpoint.USEast1;

           // config.ServiceURL = ConfigurationManager.AppSettings["SESServiceUrl"];

            AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(config);

            return client;
        }

        #endregion
    }
}
