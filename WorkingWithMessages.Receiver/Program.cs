using System.Text.Json;
using Azure.Messaging.ServiceBus;
using WorkingWithMessages.Common;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var queueName = "withmessages";

var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
{
    AutoCompleteMessages = true,
    MaxConcurrentCalls = 5
});

processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;

processor.StartProcessingAsync();
Console.ReadKey();
processor.StopProcessingAsync();
processor.CloseAsync();

async Task MessageHandler(ProcessMessageEventArgs args)
{
    var message = args.Message;
    if (string.IsNullOrWhiteSpace(message.ContentType))
    {
        Console.WriteLine(message.Body);
        return;
    }

    var deserializedMessage = JsonSerializer.Deserialize<Person>(message.Body);
    Console.WriteLine($"Person: {deserializedMessage.FirstName} {deserializedMessage.LastName}");
}

async Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine("BUMMER!");
}