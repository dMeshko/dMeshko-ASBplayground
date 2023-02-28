using Azure.Messaging.ServiceBus;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var queueName = "demoqueue";

// create service bus client
var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

// create a processor
//var processor = client.CreateProcessor(queueName);
//processor.ProcessMessageAsync += MessageHandler;
//processor.ProcessErrorAsync += ErrorHandler;

//await processor.StartProcessingAsync();
//Console.ReadKey();
//await processor.StopProcessingAsync();

// create a service bus receiver
var receiver = client.CreateReceiver(queueName);

// receive the messages
while (true)
{
    var message = await receiver.ReceiveMessageAsync();
    if (message == null)
    {
        Console.WriteLine("All messages are received!");
        break;
    }

    Console.WriteLine($"Received: {message.Body}");

    // complete the message received
    await receiver.CompleteMessageAsync(message);
}

// close the receiver
await receiver.CloseAsync();

Console.WriteLine("done!");
Console.ReadKey();