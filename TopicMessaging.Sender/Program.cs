using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using TopicsMessaging.Common;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var topicName = "orderstopic";

var adminClient = new ServiceBusAdministrationClient(connectionString);

if (!await adminClient.TopicExistsAsync(topicName))
{
    await adminClient.CreateTopicAsync(topicName);
}

var orders = new List<Order>
{
    new()
    {
        Name = "Loyal customer",
        Value = 19.99,
        Region = "USA",
        Items = 1,
        HasLoyaltyCard = true
    },
    new()
    {
        Name = "Large order",
        Value = 49.99,
        Region = "USA",
        Items = 50,
        HasLoyaltyCard = false
    },
    new()
    {
        Name = "High value",
        Value = 749.45,
        Region = "USA",
        Items = 45,
        HasLoyaltyCard = false
    },
    new()
    {
        Name = "Loyal europe",
        Value = 49.45,
        Region = "EU",
        Items = 3,
        HasLoyaltyCard = true
    },
    new()
    {
        Name = "UK order",
        Value = 49.45,
        Region = "UK",
        Items = 3,
        HasLoyaltyCard = true
    }
};

var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

var sender = client.CreateSender(topicName);

var batchMessage = await sender.CreateMessageBatchAsync();
foreach (var order in orders)
{
    Console.WriteLine($"Sending: {order}");

    var serializedOrder = JsonSerializer.Serialize(order);
    var serviceBusMessage = new ServiceBusMessage(serializedOrder);

    // add these props to enable SQL filtering
    serviceBusMessage.ApplicationProperties.Add("region", order.Region);
    serviceBusMessage.ApplicationProperties.Add("items", order.Items);
    serviceBusMessage.ApplicationProperties.Add("value", order.Value);
    serviceBusMessage.ApplicationProperties.Add("loyalty", order.HasLoyaltyCard);

    // set this to enable CorrelationId filtering
    serviceBusMessage.CorrelationId = order.Region;

    batchMessage.TryAddMessage(serviceBusMessage);
}

await sender.SendMessagesAsync(batchMessage);    

await sender.CloseAsync();