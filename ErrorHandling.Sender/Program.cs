using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ErrorHandling.Common;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var queueName = "dl-queue";

var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

var sender = client.CreateSender(queueName);

while (true)
{
    Console.WriteLine("text/json/poison/unknown/exit?");

    var messageType = Console.ReadLine().ToLower();

    if (messageType == "exit")
    {
        break;
    }

    switch (messageType)
    {
        case "text":
            await sender.SendMessageAsync(new ServiceBusMessage("Henlooo!")
            {
                ContentType = "text/plain"
            });
            break;
        case "json":
            await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(new Person("Darko", "Meshkovski")))
            {
                ContentType = "application/json"
            });
            break;
        case "poison":
            await sender.SendMessageAsync(
                new ServiceBusMessage("<person><name>Darko</name><surname>Meshkovski</surname></person>")
                {
                    ContentType = "application/json"
                });
            break;
        case "unknown":
            await sender.SendMessageAsync(new ServiceBusMessage("unknown")
            {
                ContentType = "application/unknown"
            });
            break;
        default: Console.WriteLine("What?!");
            break;
    }
}

await sender.CloseAsync();