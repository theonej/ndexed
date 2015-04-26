using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NDexed.AWS.Utility;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Models;
using NDexed.Domain.Resources;

namespace NDexed.AWS.Repository
{
    public class DynamoOrganizationRepository : IRepository<Guid, Organization>
    {
        private const string ORGANIZATION_ID_COLUMN = "OrganizationId";

        public Organization Get(Guid id)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                GetItemRequest request = CreateGetItemRequest(id);

                GetItemResponse response = client.GetItem(request);

                if (response.Item == null)
                {
                    string errorMessage = string.Format(ErrorMessages.MisingResponseItem, "Get Organization");
                    throw new MissingFieldException(errorMessage);
                }

                Organization returnValue = DynamoUtilities.GetItemFromAttributeStore<Organization>(response.Item);

                return returnValue;
            }
        }

        public Guid Add(Organization item)
        {
            if (item.OrganizationId == Guid.Empty)
                item.OrganizationId = Guid.NewGuid();

            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                PutItemRequest request = CreatePutItemRequest(item);

                client.PutItem(request);
            }

            return item.OrganizationId;
        }

        public void Remove(Organization item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                DeleteItemRequest request = CreateDeleteItemRequest(item.OrganizationId);

                client.DeleteItem(request);
            }
        }

        public IEnumerable<Organization> GetAll()
        {
            List<Organization> returnValue = new List<Organization>();

            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                ScanRequest request = CreateScanRequest();

                ScanResponse response = client.Scan(request);

                if (response.Items == null || response.Items.Any() == false)
                {
                    string errorMessage = string.Format(ErrorMessages.MisingResponseItem, "Get Organization");
                    throw new MissingFieldException(errorMessage);
                }

                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    Organization organization = DynamoUtilities.GetItemFromAttributeStore<Organization>(item);

                    returnValue.Add(organization);
                }
                return returnValue;
            }
        }
        #region Private Methods

        private PutItemRequest CreatePutItemRequest(Organization item)
        {
            PutItemRequest request = new PutItemRequest();

            request.TableName = ConfigurationManager.AppSettings["organizationTableName"];

            request.Item = new Dictionary<string, AttributeValue>();

            request.Item.Add(ORGANIZATION_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.OrganizationId));
            request.Item.Add(DynamoUtilities.SERIALIZED_DATA_COLUMN, DynamoUtilities.GetItemAttributeSerializedValue(item));

            return request;
        }

        private GetItemRequest CreateGetItemRequest(Guid id)
        {
            GetItemRequest request = new GetItemRequest();

            request.TableName = ConfigurationManager.AppSettings["organizationTableName"];

            request.Key = new Dictionary<string, AttributeValue>()
            { 
              {ORGANIZATION_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(id) }
            };

            return request;
        }

        private DeleteItemRequest CreateDeleteItemRequest(Guid id)
        {
            DeleteItemRequest request = new DeleteItemRequest();

            request.TableName = ConfigurationManager.AppSettings["organizationTableName"];
            request.Key = new Dictionary<string, AttributeValue>()
            { 
              {ORGANIZATION_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(id) }
            };

            return request;
        }

        private ScanRequest CreateScanRequest()
        {
            ScanRequest request = new ScanRequest();

            request.TableName = ConfigurationManager.AppSettings["organizationTableName"];
            request.ScanFilter = new Dictionary<string, Condition>
            {
               
            };

            return request;
        }

        #endregion

    }
}
