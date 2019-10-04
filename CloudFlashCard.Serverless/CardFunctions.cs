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
using Amazon.DynamoDBv2.DocumentModel;

using Newtonsoft.Json;

namespace CloudFlashCard.Serverless
{
    public class CardFunctions
    {
        const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "CloudFlashCard-Card";

        public const string ID_QUERY_STRING_NAME = "CardId";
        public const string TOPIC_ID_QUERY_STRING_NAME = "TopicId";
        public const string USER_ID_QUERY_STRING_NAME = "UserId";
        public const string CONTENT_QUERY_STRING_NAME = "Content";

        private IDynamoDBContext DDBContext { get; set; }

        public CardFunctions()
        {
            var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Topic)] = new Amazon.Util.TypeMapping(typeof(Topic), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public async Task<APIGatewayProxyResponse> GetFlashCardsPerTopicAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Getting flash cards per topic");

            string topicId = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(TOPIC_ID_QUERY_STRING_NAME))
            {
                topicId = request.PathParameters[TOPIC_ID_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(TOPIC_ID_QUERY_STRING_NAME))
            {
                topicId = request.QueryStringParameters[TOPIC_ID_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(topicId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {TOPIC_ID_QUERY_STRING_NAME}"
                };
            }

            List<Document> page = null;

            using (var dynamoDb = new AmazonDynamoDBClient(RegionEndpoint.USEast2))
            {
                DynamoDBContext dbContext = new DynamoDBContext(dynamoDb);
                Table table = Table.LoadTable(dynamoDb, TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

                ScanFilter scanFilter = new ScanFilter();
                scanFilter.AddCondition("TopicId", ScanOperator.Equal, topicId);

                ScanOperationConfig config = new ScanOperationConfig()
                {
                    Filter = scanFilter
                };

                Search search = table.Scan(config);
                page = await search.GetRemainingAsync();
            }

            context.Logger.LogLine($"Found {page.Count} flash cards");

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(page),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }

        public async Task<APIGatewayProxyResponse> GetFlashCardAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string flashCardId = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
            {
                flashCardId = request.PathParameters[ID_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
            {
                flashCardId = request.QueryStringParameters[ID_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(flashCardId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
                };
            }

            context.Logger.LogLine($"Getting topic {flashCardId}");

            Document document = null;
            List<Document> page = null;

            using (var dynamoDb = new AmazonDynamoDBClient(RegionEndpoint.USEast2))
            {
                Table table = Table.LoadTable(dynamoDb, TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

                var scanFilter = new ScanFilter();
                scanFilter.AddCondition("CardId", ScanOperator.Equal, flashCardId);

                Search ageSearch = table.Scan(scanFilter);

                page = await ageSearch.GetRemainingAsync();

                if (page.Count > 0)
                {
                    document = page[0];
                }
            }

            context.Logger.LogLine($"Found flash card: {document != null}");

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
                Body = JsonConvert.SerializeObject(document),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
            return response;
        }

        public async Task<APIGatewayProxyResponse> CreateFlashCardAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var entity = JsonConvert.DeserializeObject<FlashCard>(request?.Body);
            entity.CardId = Guid.NewGuid().ToString();
            entity.CreateDate = DateTime.Now;
            entity.UpdateDate = DateTime.Now;

            // Check if the topic id is passed as parameter
            string topicId = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(TOPIC_ID_QUERY_STRING_NAME))
            {
                topicId = request.PathParameters[TOPIC_ID_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(TOPIC_ID_QUERY_STRING_NAME))
            {
                topicId = request.QueryStringParameters[TOPIC_ID_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(topicId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {TOPIC_ID_QUERY_STRING_NAME}"
                };
            }

            entity.TopicId = topicId;

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

            // Check if the content is passed as parameter
            string content = null;

            if (request.PathParameters != null && request.PathParameters.ContainsKey(CONTENT_QUERY_STRING_NAME))
            {
                content = request.PathParameters[CONTENT_QUERY_STRING_NAME];
            }
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(CONTENT_QUERY_STRING_NAME))
            {
                content = request.QueryStringParameters[CONTENT_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(content))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {CONTENT_QUERY_STRING_NAME}"
                };
            }

            entity.Content = content;

            // Put the item in DynamoDB table
            context.Logger.LogLine($"Saving flash card with id {entity.CardId}");

            var dynamoDb = new AmazonDynamoDBClient(RegionEndpoint.USEast2);
            Table table = Table.LoadTable(dynamoDb, TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);

            Document topic = new Document();
            topic["UserId"] = entity.UserId;
            topic["TopicId"] = entity.TopicId;
            topic["CardId"] = entity.CardId;
            topic["Content"] = entity.Content;
            topic["CreateDate"] = entity.CreateDate;
            topic["UpdateDate"] = entity.CreateDate;

            await table.PutItemAsync(topic);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(entity.CardId),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }
    }
}
