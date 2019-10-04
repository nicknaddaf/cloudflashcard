using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using Newtonsoft.Json;
using Amazon.DynamoDBv2.DocumentModel;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CloudFlashCard.Serverless
{
    public class TopicFunctions
    {
        private const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "CloudFlashCard-Topic";

        public const string ID_QUERY_STRING_NAME = "TopicId";
        public const string USER_ID_QUERY_STRING_NAME = "UserId";
        public const string NAME_QUERY_STRING_NAME = "Name";

        private IDynamoDBContext DDBContext { get; set; }

        public TopicFunctions()
        {
            var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Topic)] = new Amazon.Util.TypeMapping(typeof(Topic), TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };

            var dynamoDb = new AmazonDynamoDBClient(RegionEndpoint.USEast2);
            

            DDBContext = new DynamoDBContext(dynamoDb, config);
        }

        public async Task<APIGatewayProxyResponse> GetTopicsPerUserAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Getting topics per user");

            // Check if the user id is passed as parameter
            string userId = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(USER_ID_QUERY_STRING_NAME))
            {
                userId = request.PathParameters[USER_ID_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(USER_ID_QUERY_STRING_NAME))
            {
                userId = request.QueryStringParameters[USER_ID_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(userId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {USER_ID_QUERY_STRING_NAME}"
                };
            }

            List<Document> page = null;

            using (var dynamoDb = new AmazonDynamoDBClient(RegionEndpoint.USEast2))
            {
                DynamoDBContext dbContext = new DynamoDBContext(dynamoDb);
                Table table = Table.LoadTable(dynamoDb, TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

                ScanFilter scanFilter = new ScanFilter();
                scanFilter.AddCondition("UserId", ScanOperator.Equal, userId);

                ScanOperationConfig config = new ScanOperationConfig()
                {
                    //AttributesToGet = new List<string> { "Subject", "Message" },
                    Filter = scanFilter
                };

                Search search = table.Scan(config);
                page = await search.GetRemainingAsync();
            }

            context.Logger.LogLine($"Found {page.Count} topics");

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(page),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }

        public async Task<APIGatewayProxyResponse> GetTopicAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string topicId = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
            {
                topicId = request.PathParameters[ID_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
            {
                topicId = request.QueryStringParameters[ID_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(topicId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
                };
            }

            context.Logger.LogLine($"Getting topic {topicId}");

            Document document = null;
            List<Document> page = null;

            using (var dynamoDb = new AmazonDynamoDBClient(RegionEndpoint.USEast2))
            {
                Table table = Table.LoadTable(dynamoDb, TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

                
                var scanFilter = new ScanFilter();
                scanFilter.AddCondition("TopicId", ScanOperator.Equal, topicId);

                Search ageSearch = table.Scan(scanFilter);

                page = await ageSearch.GetRemainingAsync();

                if(page.Count > 0)
                {
                    document = page[0];
                }
            }

            context.Logger.LogLine($"Found topic: {document != null}");

            if (document == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(page[0]),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
            return response;
        }

        public async Task<APIGatewayProxyResponse> CreateTopicAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var entity = JsonConvert.DeserializeObject<Topic>(request?.Body);
            entity.TopicId = Guid.NewGuid().ToString();
            entity.CreateDate = DateTime.Now;

            // Check if the user id is passed as parameter
            string userId = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(USER_ID_QUERY_STRING_NAME))
            {
                userId = request.PathParameters[USER_ID_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(USER_ID_QUERY_STRING_NAME))
            {
                userId = request.QueryStringParameters[USER_ID_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(userId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {USER_ID_QUERY_STRING_NAME}"
                };
            }

            entity.UserId = userId;

            // Check if the name is passed as parameter
            string name = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(NAME_QUERY_STRING_NAME))
            {
                name = request.PathParameters[NAME_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(NAME_QUERY_STRING_NAME))
            {
                name = request.QueryStringParameters[NAME_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(name))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {NAME_QUERY_STRING_NAME}"
                };
            }

            entity.Name = name;

            // Put the item in DynamoDB table
            context.Logger.LogLine($"Saving topic with id {entity.TopicId}");

            var dynamoDb = new AmazonDynamoDBClient(RegionEndpoint.USEast2);
            Table table = Table.LoadTable(dynamoDb, TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

            Document topic = new Document();
            topic["UserId"] = entity.UserId;
            topic["TopicId"] = entity.TopicId;
            topic["Name"] = entity.Name;
            topic["CreateDate"] = entity.CreateDate;
            topic["UpdateDate"] = entity.CreateDate;
            
            await table.PutItemAsync(topic);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(entity.TopicId),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }
    }
}
