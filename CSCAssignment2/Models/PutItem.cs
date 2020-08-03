using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCAssignment2.Models
{
    public class PutItem : IPutItem
    {
        private readonly IAmazonDynamoDB _dynamoClient;

        public PutItem(IAmazonDynamoDB dynamoClient)
        {
            _dynamoClient = dynamoClient;
        }

        public async Task AddNewEntry(string subscriptionId, string invoiceId, int userId)
        {
            var queryRequest = RequestBuilder(subscriptionId, invoiceId, userId);

            await PutItemAsync(queryRequest);
        }

        private PutItemRequest RequestBuilder(string subscriptionId, string invoiceId, int userId)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                {"subscriptionId", new AttributeValue {S = subscriptionId}},
                {"invoiceId", new AttributeValue {S = invoiceId}},
                {"userId", new AttributeValue {N = userId.ToString()}}
            };

            return new PutItemRequest
            {
                TableName = "Payment",
                Item = item
            };
        }

        private async Task PutItemAsync(PutItemRequest request)
        {
            await _dynamoClient.PutItemAsync(request);
        }
    }
}
