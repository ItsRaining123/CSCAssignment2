using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCAssignment2.Models
{
    public class GetItem : IGetItem
    {
        private readonly IAmazonDynamoDB _dynamoClient;

        public GetItem(IAmazonDynamoDB dynamoClient)
        {
            _dynamoClient = dynamoClient;
        }

        public async Task<DyanmoTableItems> GetItems(int? id)
        {
            var queryRequest = RequestBuilder(id);

            var result = await ScanAsync(queryRequest);

            return new DyanmoTableItems
            {
                Items = result.Items.Select(Map).ToList()
            };
        }
        private async Task<ScanResponse> ScanAsync(ScanRequest request)
        {
            var response = await _dynamoClient.ScanAsync(request);

            return response;
        }
        private Item Map(Dictionary<string, AttributeValue> result)
        {
            return new Item
            {
                Id = Convert.ToInt32(result["Id"].N),
                subscriptionId = result["subscriptionId"].S,
                invoiceId = result["invoiceId"].S
            };
        }
        private ScanRequest RequestBuilder(int? id)
        {
            if (id.HasValue == false)
            {
                return new ScanRequest
                {
                    TableName = "Payment",
                };
            }

            return new ScanRequest
            {
                TableName = "Payment",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>  {
            {
                ":v_Id", new AttributeValue { N = id.ToString() }}
            },
                FilterExpression = "Id = :v_Id",
                ProjectionExpression = "Id, subscriptionId, invoiceId"
            };
        }
    }

    public class DyanmoTableItems
    {
        public IEnumerable<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string subscriptionId { get; set; }
        public string invoiceId { get; set; }
    }
}
