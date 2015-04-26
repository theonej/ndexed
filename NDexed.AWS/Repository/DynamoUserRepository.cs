using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NDexed.Domain.Models.User;
using NDexed.DataAccess.Repositories;
using NDexed.Domain.Resources;
using NDexed.AWS.Utility;
using System.Configuration;

namespace NDexed.AWS.Repository
{
    public class DynamoUserRepository : IRepository<UserInfo, UserInfo>,
                                        ISearchableRepository<UserInfo, UserInfo>
    {
        private const string USER_ID_COLUMN = "UserId";
        private const string ORGANIZATION_ID_COLUMN = "OrganizationId";
        private const string USER_NAME_COLUMN = "UserName";
        private const string EMAIL_ADDRESS_COLUMN = "EmailAddress";
        private const string PASSWORD_COLUMN = "Password";

        #region Public Methods

        public UserInfo Get(UserInfo item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                ScanRequest request = CreateScanRequest(item.Id);
                if (request.ScanFilter.Count == 0)
                {
                    throw new InvalidOperationException(ErrorMessages.NoSearchCriteria);
                }

                    ScanResponse response = client.Scan(request);

                    if (response.Items == null)
                    {
                        string errorMessage = string.Format(ErrorMessages.MisingResponseItem, "Get User");
                        throw new MissingFieldException(errorMessage);
                    }

                Dictionary<string, AttributeValue> userData = response.Items[0];

                UserInfo returnValue = DynamoUtilities.GetItemFromAttributeStore<UserInfo>(userData);

                return returnValue;
            }
        }


        public IEnumerable<UserInfo> GetAll()
        {
            throw new NotImplementedException();
        }

        public UserInfo Add(UserInfo item)
        {
            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }

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

        /// <summary>
        /// This method will use whatever information is supplied to scan the table
        /// supported fields are:
        /// CustomerId
        /// UserName
        /// Password(Hash)
        /// EmailAddress
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public IEnumerable<UserInfo> Search(UserInfo searchCriteria)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                ScanRequest request = CreateScanRequest(searchCriteria);
                if (request.ScanFilter.Count == 0)
                {
                    throw new InvalidOperationException(ErrorMessages.NoSearchCriteria);
                }

                List<UserInfo> returnValue = new List<UserInfo>();

                try
                {
                    ScanResponse response = client.Scan(request);

                    if (response.Items == null)
                    {
                        string errorMessage = string.Format(ErrorMessages.MisingResponseItem, "Search User");
                        throw new MissingFieldException(errorMessage);
                    }

                    foreach (Dictionary<string, AttributeValue> item in response.Items)
                    {
                        UserInfo user = DynamoUtilities.GetItemFromAttributeStore<UserInfo>(item);
                        returnValue.Add(user);
                    }
                }
                catch (AmazonDynamoDBException ex)
                {
                    //expected exception, 404
                }

                return returnValue;
            }

            throw new NotImplementedException();
        }

        public void Remove(UserInfo item)
        {
            AmazonDynamoDBClient client = DynamoUtilities.InitializeClient();
            using (client)
            {
                DeleteItemRequest request = CreateDeleteItemRequest(item);

                client.DeleteItem(request);
            }
        }

        #endregion

        #region Private Methods

        private static PutItemRequest CreatePutItemRequest(UserInfo item)
        {
            PutItemRequest request = new PutItemRequest();

            request.TableName = ConfigurationManager.AppSettings["userTableName"];

            request.Item = new Dictionary<string, AttributeValue>();

            request.Item.Add(USER_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.Id));
            request.Item.Add(ORGANIZATION_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.OrganizationId));
            request.Item.Add(EMAIL_ADDRESS_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.EmailAddress));
            request.Item.Add(USER_NAME_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.UserName));
            request.Item.Add(PASSWORD_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.PasswordHash));
            request.Item.Add(DynamoUtilities.SERIALIZED_DATA_COLUMN, DynamoUtilities.GetItemAttributeSerializedValue(item));

            return request;
        }

        private static DeleteItemRequest CreateDeleteItemRequest(UserInfo item)
        {
            DeleteItemRequest request = new DeleteItemRequest();

            request.TableName = ConfigurationManager.AppSettings["userTableName"]; ;

            request.Key = new Dictionary<string, AttributeValue>
            {
                {USER_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.Id)},
                {ORGANIZATION_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.OrganizationId)}
            };

            return request;
        }

        private static GetItemRequest CreateGetItemRequest(UserInfo item)
        {
            GetItemRequest request = new GetItemRequest();

            request.TableName = ConfigurationManager.AppSettings["userTableName"]; ;

            request.Key = new Dictionary<string, AttributeValue>
            {
                {USER_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.Id)},
                {ORGANIZATION_ID_COLUMN, DynamoUtilities.GetItemAttributeStringValue(item.OrganizationId)}
            };

            return request;
        }

        private static ScanRequest CreateScanRequest(UserInfo searchCriteria)
        {
            ScanRequest request = new ScanRequest();

            request.TableName = ConfigurationManager.AppSettings["userTableName"]; ;

            request.ScanFilter = new Dictionary<string, Condition>();

            if (! string.IsNullOrEmpty(searchCriteria.EmailAddress))
            {
                Condition condition = new Condition();
                condition.ComparisonOperator = DynamoUtilities.DYNAMO_EQUALITY_OPERATOR;
                condition.AttributeValueList = new List<AttributeValue>()
                        {
                            DynamoUtilities.GetItemAttributeStringValue(searchCriteria.EmailAddress.ToLower())
                        };

                request.ScanFilter.Add(EMAIL_ADDRESS_COLUMN, condition);
            }

            if (!string.IsNullOrEmpty(searchCriteria.PasswordHash))
            {
                Condition condition = new Condition();
                condition.ComparisonOperator = DynamoUtilities.DYNAMO_EQUALITY_OPERATOR;
                condition.AttributeValueList = new List<AttributeValue>()
                        {
                            DynamoUtilities.GetItemAttributeStringValue(searchCriteria.PasswordHash)
                        };

                request.ScanFilter.Add(PASSWORD_COLUMN, condition);
            }

            if (!string.IsNullOrEmpty(searchCriteria.UserName))
            {
                Condition condition = new Condition();
                condition.ComparisonOperator = DynamoUtilities.DYNAMO_EQUALITY_OPERATOR;
                condition.AttributeValueList = new List<AttributeValue>()
                        {
                            DynamoUtilities.GetItemAttributeStringValue(searchCriteria.UserName)
                        };

                request.ScanFilter.Add(USER_NAME_COLUMN, condition);
            }

            if (searchCriteria.Id != Guid.Empty)
            {
                Condition condition = new Condition();
                condition.ComparisonOperator = DynamoUtilities.DYNAMO_EQUALITY_OPERATOR;
                condition.AttributeValueList = new List<AttributeValue>()
                        {
                            DynamoUtilities.GetItemAttributeStringValue(searchCriteria.Id)
                        };

                request.ScanFilter.Add(USER_ID_COLUMN, condition);
            }

            return request;
        }

        private static ScanRequest CreateScanRequest(Guid userId)
        {
            ScanRequest request = new ScanRequest();

            request.TableName = ConfigurationManager.AppSettings["userTableName"];
            
            request.ScanFilter = new Dictionary<string, Condition>();

            Condition condition = new Condition();
            condition.ComparisonOperator = DynamoUtilities.DYNAMO_EQUALITY_OPERATOR;
            condition.AttributeValueList = new List<AttributeValue>()
            {
                DynamoUtilities.GetItemAttributeStringValue(userId)
            };

            request.ScanFilter.Add(USER_ID_COLUMN, condition);

            return request;
        }

        #endregion

    }
}
