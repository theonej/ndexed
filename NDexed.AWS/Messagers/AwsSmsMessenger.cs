using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using NDexed.Messaging.Messages;
namespace NDexed.AWS.Messagers
{
    public class AwsSmsMessenger : IMessager
    {
        public void SendMessage(IMessageInfo message)
        {
            AmazonSimpleNotificationServiceClient client = InitializeClient();
            using (client)
            {
               
            }
        }

        #region Private Methods

        private AmazonSimpleNotificationServiceClient InitializeClient()
        {
            AmazonSimpleNotificationServiceConfig config = new AmazonSimpleNotificationServiceConfig();
            config.RegionEndpoint = RegionEndpoint.USEast1;

            // config.ServiceURL = ConfigurationManager.AppSettings["SESServiceUrl"];

            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient(config);

            return client;
        }
        #endregion
    }
}
