using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DynamoDbDataExporter;

Console.WriteLine($"Starting the export process! on Table: {Configuration.TABLE_NAME}");
var ssoCreds = SsoHelper.LoadSsoCredentials(Configuration.SSO_PROFILE);
await SsoHelper.PrintCredentials(ssoCreds);

var dynamoDBClient = new AmazonDynamoDBClient(ssoCreds);
var _context = new DynamoDBContext(dynamoDBClient);
var config = new DynamoDBOperationConfig
{
    OverrideTableName = Configuration.TABLE_NAME,
};

// Given data 
var testDbEntity = new TestDbEntity
{
    Pk = "TestEntity1",
    Orders = new()
    {
        new()
        {
            OrderId="100",
            City="Stockholm",
            PostalCode="11130",
            OrderLastModifiedUnixTime=3,
            PropertyThatShouldNeverBeTouched="Update should not change this prop"

        },
        new()
        {
            OrderId="99",
            City="London",
            PostalCode="11131",
            OrderLastModifiedUnixTime=3,
            PropertyThatShouldNeverBeTouched="Update should not change this prop"
        },
        new()
        {
            OrderId="101",
            City="Islamabad",
            PostalCode="11132",
            OrderLastModifiedUnixTime=30,
        }
    }
};

// Insert data
await _context.SaveAsync(testDbEntity, new DynamoDBOperationConfig { OverrideTableName = Configuration.TABLE_NAME });

// Perform updates
Console.WriteLine("Running" + nameof(UpdateOrderId100AndAllProperties));
await UpdateOrderId100AndAllProperties();
Console.WriteLine("Check DB all values should be updated for [0] OrderId100");
Console.ReadLine();

Console.WriteLine("Running" + nameof(UpdateOrderId99AndAllProperties));
await UpdateOrderId99AndAllProperties();
Console.WriteLine("Check DB all values should be updated for [1] OrderId99");
Console.ReadLine();

Console.WriteLine("Running" + nameof(UpdateOrderId101ShouldFailWhenLastModifiedIsInPast));
await UpdateOrderId101ShouldFailWhenLastModifiedIsInPast();

Console.WriteLine("Running" + nameof(UpdateOrderId100FailsWhenOrderIdIsWrong));
await UpdateOrderId100FailsWhenOrderIdIsWrong();

async Task UpdateOrderId100AndAllProperties()
{
    var updateItemRequest = new UpdateItemRequest()
    {
        TableName = Configuration.TABLE_NAME,
        Key = new Dictionary<string, AttributeValue>
            {
                { "Pk", new AttributeValue { S = testDbEntity.Pk } },
            },
        UpdateExpression = $"SET Orders[0].City = :city, Orders[0].PostalCode = :postalCode, Orders[0].OrderLastModifiedUnixTime = :orderLastModifiedUnixTime",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":city", new AttributeValue { S = "Hunza" } },
                { ":postalCode", new AttributeValue { S = "17670" } },
                { ":orderLastModifiedUnixTime", new AttributeValue { N = $"5" } },
                { ":orderId", new AttributeValue { S = testDbEntity.Orders[0].OrderId } },
            },
        ConditionExpression = $"Orders[0].OrderLastModifiedUnixTime < :orderLastModifiedUnixTime AND Orders[0].OrderId = :orderId",
    };

    await dynamoDBClient.UpdateItemAsync(updateItemRequest);
}

async Task UpdateOrderId99AndAllProperties()
{
    var updateItemRequest = new UpdateItemRequest()
    {
        TableName = Configuration.TABLE_NAME,
        Key = new Dictionary<string, AttributeValue>
            {
                { "Pk", new AttributeValue { S = testDbEntity.Pk } },
            },
        UpdateExpression = $"SET Orders[1].City = :city, Orders[1].PostalCode = :postalCode, Orders[1].OrderLastModifiedUnixTime = :orderLastModifiedUnixTime",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":city", new AttributeValue { S = "Gilgit" } },
                { ":postalCode", new AttributeValue { S = "17671" } },
                { ":orderLastModifiedUnixTime", new AttributeValue { N = $"6" } },
                { ":orderId", new AttributeValue { S = testDbEntity.Orders[1].OrderId } },
            },
        ConditionExpression = $"Orders[1].OrderLastModifiedUnixTime < :orderLastModifiedUnixTime AND Orders[1].OrderId = :orderId",
    };

    await dynamoDBClient.UpdateItemAsync(updateItemRequest);
}

async Task UpdateOrderId101ShouldFailWhenLastModifiedIsInPast()
{
    var updateItemRequest = new UpdateItemRequest()
    {
        TableName = Configuration.TABLE_NAME,
        Key = new Dictionary<string, AttributeValue>
            {
                { "Pk", new AttributeValue { S = testDbEntity.Pk } },
            },
        UpdateExpression = $"SET Orders[2].City = :city, Orders[2].PostalCode = :postalCode, Orders[2].OrderLastModifiedUnixTime = :orderLastModifiedUnixTime",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":city", new AttributeValue { S = "Kallhall" } },
                { ":postalCode", new AttributeValue { S = "17672" } },
                { ":orderLastModifiedUnixTime", new AttributeValue { N = $"7" } },
                { ":orderId", new AttributeValue { S = testDbEntity.Orders[2].OrderId } },
            },
        ConditionExpression = $"Orders[2].OrderLastModifiedUnixTime < :orderLastModifiedUnixTime AND Orders[2].OrderId = :orderId",
    };

    try
    {
        await dynamoDBClient.UpdateItemAsync(updateItemRequest);
    }
    catch (ConditionalCheckFailedException)
    {
        Console.WriteLine("Ignoring the update because last modified was in past");
    }
}

async Task UpdateOrderId100FailsWhenOrderIdIsWrong()
{
    var updateItemRequest = new UpdateItemRequest()
    {
        TableName = Configuration.TABLE_NAME,
        Key = new Dictionary<string, AttributeValue>
            {
                { "Pk", new AttributeValue { S = testDbEntity.Pk } },
            },
        UpdateExpression = $"SET Orders[0].City = :city, Orders[0].PostalCode = :postalCode, Orders[0].OrderLastModifiedUnixTime = :orderLastModifiedUnixTime",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":city", new AttributeValue { S = "Hunza" } },
                { ":postalCode", new AttributeValue { S = "17670" } },
                { ":orderLastModifiedUnixTime", new AttributeValue { N = $"10" } },
                { ":orderId", new AttributeValue { S = "101" } },
            },
        ConditionExpression = $"Orders[0].OrderLastModifiedUnixTime < :orderLastModifiedUnixTime AND Orders[0].OrderId = :orderId",
    };

    try
    {
        await dynamoDBClient.UpdateItemAsync(updateItemRequest);
    }
    catch (ConditionalCheckFailedException)
    {
        Console.WriteLine("Ignoring the update because order id is wrong");
    }
}

public class TestDbEntity
{
    public string Pk { get; set; } = null!;
    public List<OrderEntity> Orders { get; set; }

    public class OrderEntity
    {
        public string OrderId { get; set; } = null!;
        public string PostalCode { get; set; }
        public string City { get; set; } = null!;
        public int OrderLastModifiedUnixTime { get; set; }
        public string PropertyThatShouldNeverBeTouched { get; set; }
    }
}
