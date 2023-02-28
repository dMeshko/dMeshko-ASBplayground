using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var topicName = "chattopic";

Console.WriteLine($"Enter name: ");
var username = Console.ReadLine();

// create an administration client to manage artifacts
var serviceBusAdministrationClient = new ServiceBusAdministrationClient(connectionString);

// create a topic if it doesn't exist
if (!await serviceBusAdministrationClient.TopicExistsAsync(topicName))
{
    await serviceBusAdministrationClient.CreateTopicAsync(topicName);
}

// create a subscriptoin if it doesn't exist
if (!await serviceBusAdministrationClient.SubscriptionExistsAsync(topicName, username))
{
    await serviceBusAdministrationClient.CreateSubscriptionAsync(new CreateSubscriptionOptions(topicName, username)
    {
        AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
    });
}

// create a service bus client
var serviceBusClient = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

// create a service bus sender
var sender = serviceBusClient.CreateSender(topicName);

// create a message processor
var processor = serviceBusClient.CreateProcessor(topicName, username);

// add handler to process messages
processor.ProcessMessageAsync += MessageHandler;

// add handler to process errors
processor.ProcessErrorAsync += ErrorHandler;

// start the message processor
await processor.StartProcessingAsync();

// send a hello message
var helloMessage = new ServiceBusMessage($"Hello from {username}!");
await sender.SendMessageAsync(helloMessage);

while (true)
{
    var text = Console.ReadLine();

    if (text == "exit")
    {
        break;
    }

    var message = new ServiceBusMessage(text);
    await sender.SendMessageAsync(message);
}

// send a goodbye message
var goodbyeMessage = new ServiceBusMessage($"Goodbye {username}");
await sender.SendMessageAsync(goodbyeMessage);

await processor.StopProcessingAsync();

await processor.CloseAsync();
await sender.CloseAsync();

Console.ReadKey();


// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // complete the message. message is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}