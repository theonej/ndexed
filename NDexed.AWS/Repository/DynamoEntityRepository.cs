using System;
using System.Collections.Generic;
using System.Configuration;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NDexed.AWS.Utility;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models.Data;
using NDexed.Domain.Resources;

namespace NDexed.AWS.Repository
{
    class DynamoDataRepository : IRepository<DataInfo, DataInfo>
    {
        private const string DATA_ID_COLUMN = "DataId";
        private const string ORGANIZATION_ID = "OrganizationId";

        public DataInfo Get(DataInfo item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                ScanRequest request = CreateScanRequest(item.DataId);
                if (request.ScanFilter.Count == 0)
                {
                    throw new InvalidOperationException(ErrorMessages.NoSearchCriteria);
                }

                ScanResponse response = client.Scan(request);

                if (response.Items == null)
                {
                    string errorMessage = string.Format(ErrorMessages.MisingResponseItem, "Get Data Info");
                    throw new MissingFieldException(errorMessage);
                }

                Dictionary<string, AttributeValue> userData = response.Items[0];

                var returnValue = DynamoUtilities.GetItemFromAttributeStore<DataInfo>(userData);

                return returnValue;
            }
        }

        public DataInfo Add(DataInfo item)
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

        public void Remove(DataInfo item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                DeleteItemRequest request = CreateDeleteItemRequest(item);

                client.DeleteItem(request);
            }
        }

        public IEnumerable<DataInfo> GetAll()
        {
            throw new System.NotImplementedException();
        }

        #region Private Methods

        private static ScanRequest CreateScanRequest(Guid userId)
        {
            ScanRequest request = new ScanRequest();

            request.TableName = ConfigurationManager.AppSettings["dataInfoTableName"];

            request.ScanFilter = new Dictionary<string, Condition>();

            Condition condition = new Condition();
            condition.ComparisonOperator = DynamoUtilities.DYNAMO_EQUALITY_OPERATOR;
            condition.AttributeValueList = new List<AttributeValue>()
            {
                DynamoUtilities.GetItemAttributeStringValue(userId)
            };

            request.ScanFilter.Add(DATA_ID_COLUMN, condition);

            return request;
        }


        private static PutItemRequest CreatePutItemRequest(DataInfo item)
        {
            PutItemRequest request = new PutItemRequest();

            request.TableName = ConfigurationManager.AppSettings["dataInfoTableName"];

            request.Item = new Dictionary<string, AttributeValue>();

            request.Item.Add(DATA_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.DataId));
            request.Item.Add(ORGANIZATION_ID, DynamoUtilities.GetItemAttributeStringValue(item.OrganizationId));
            request.Item.Add(DynamoUtilities.SERIALIZED_DATA_COLUMN, DynamoUtilities.GetItemAttributeSerializedValue(item));

            return request;
        }


        private static DeleteItemRequest CreateDeleteItemRequest(DataInfo item)
        {
            var request = new DeleteItemRequest();

            request.TableName = ConfigurationManager.AppSettings["dataInfoTableName"]; ;

            request.Key = new Dictionary<string, AttributeValue>
            {
                {DATA_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.DataId)},
                {ORGANIZATION_ID, DynamoUtilities.GetItemAttributeStringValue(item.OrganizationId)}
            };

            return request;
        }

        #endregion
    }
}
