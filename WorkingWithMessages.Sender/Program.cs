using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using WorkingWithMessages.Common;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var queueName = "withmessages";

var adminClient = new ServiceBusAdministrationClient(connectionString);
if (!await adminClient.QueueExistsAsync(queueName))
{
    await adminClient.CreateQueueAsync(new CreateQueueOptions(queueName)
    {
        RequiresDuplicateDetection = true,
        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(1)
    });
}

var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

var sender = client.CreateSender(queueName);
var batchMessage = await sender.CreateMessageBatchAsync();

batchMessage.TryAddMessage(new ServiceBusMessage("hello")
{
    MessageId = "1"
});
batchMessage.TryAddMessage(new ServiceBusMessage("world")
{
    MessageId = "2"
});

// person message
var person = new Person
{
    FirstName = "Darko",
    LastName = "Meshkovski"
};
var serializedPerson = JsonSerializer.Serialize(person);
var personMessage = new ServiceBusMessage(serializedPerson)
{
    MessageId = "3",
    ContentType = typeof(Person).AssemblyQualifiedName
};
batchMessage.TryAddMessage(personMessage);

await sender.SendMessagesAsync(batchMessage);
await sender.CloseAsync();