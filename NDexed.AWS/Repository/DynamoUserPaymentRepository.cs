using System;
using System.Collections.Generic;
using System.Configuration;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NDexed.AWS.Utility;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.Payment;
using NDexed.Domain.Resources;

namespace NDexed.AWS.Repository
{
    public class DynamoUserPaymentRepository : IRepository<UserPaymentInfo, UserPaymentInfo>
    {
        private const string USER_PAYMENT_ID_COLUMN = "UserPaymentInfoId";
        private const string USER_ID_COLUMN = "UserId";

        public UserPaymentInfo Get(UserPaymentInfo item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                ScanRequest request = CreateScanRequest(item.UserPaymentInfoId);
                if (request.ScanFilter.Count == 0)
                {
                    throw new InvalidOperationException(ErrorMessages.NoSearchCriteria);
                }

                ScanResponse response = client.Scan(request);

                if (response.Items == null)
                {
                    string errorMessage = string.Format(ErrorMessages.MisingResponseItem, "Get User Payment Info");
                    throw new MissingFieldException(errorMessage);
                }

                Dictionary<string, AttributeValue> userData = response.Items[0];

                var returnValue = DynamoUtilities.GetItemFromAttributeStore<UserPaymentInfo>(userData);

                return returnValue;
            }
        }

        public UserPaymentInfo Add(UserPaymentInfo item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                PutItemRequest request = CreatePutItemRequest(item);

                try
                {
                    client.PutItem(request);

                    return item;
                }
                catch (AmazonDynamoDBException ex)
                {
                    throw new MissingFieldException(ex.Message);
                }
            }
        }

        public void Remove(UserPaymentInfo item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                DeleteItemRequest request = CreateDeleteItemRequest(item);

                client.DeleteItem(request);
            }
        }

        public IEnumerable<UserPaymentInfo> GetAll()
        {
            throw new System.NotImplementedException();
        }

        #region Private Methods

        private static ScanRequest CreateScanRequest(Guid userId)
        {
            ScanRequest request = new ScanRequest();

            request.TableName = ConfigurationManager.AppSettings["userPaymentInfoTableName"];
            
            request.ScanFilter = new Dictionary<string, Condition>();

            Condition condition = new Condition();
            condition.ComparisonOperator = DynamoUtilities.DYNAMO_EQUALITY_OPERATOR;
            condition.AttributeValueList = new List<AttributeValue>()
            {
                DynamoUtilities.GetItemAttributeStringValue(userId)
            };

            request.ScanFilter.Add(USER_PAYMENT_ID_COLUMN, condition);

            return request;
        }


        private static PutItemRequest CreatePutItemRequest(UserPaymentInfo item)
        {
            PutItemRequest request = new PutItemRequest();

            request.TableName = ConfigurationManager.AppSettings["userPaymentInfoTableName"];

            request.Item = new Dictionary<string, AttributeValue>();

            request.Item.Add(USER_PAYMENT_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.UserPaymentInfoId));
            request.Item.Add(USER_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.UserId));
            request.Item.Add(DynamoUtilities.SERIALIZED_DATA_COLUMN, DynamoUtilities.GetItemAttributeSerializedValue(item));

            return request;
        }


        private static DeleteItemRequest CreateDeleteItemRequest(UserPaymentInfo item)
        {
            var request = new DeleteItemRequest();

            request.TableName = ConfigurationManager.AppSettings["userPaymentInfoTableName"]; ;

            request.Key = new Dictionary<string, AttributeValue>
            {
                {USER_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.UserId)},
                {USER_PAYMENT_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.UserPaymentInfoId)}
            };

            return request;
        }

        #endregion
    }
}
