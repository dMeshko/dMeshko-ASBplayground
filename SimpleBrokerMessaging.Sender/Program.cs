using System.Diagnostics;
using Azure.Messaging.ServiceBus;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var queueName = "demoqueue";

var sentance = "Microsoft";

try
{
    // create a service bus client
    var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    });

    // create a service bus sender
    var sender = client.CreateSender(queueName);

    // send some message
    foreach (var character in sentance)
    {
        var message = new ServiceBusMessage(character.ToString());
        await sender.SendMessageAsync(message);
        Console.WriteLine($"Sent {character}");
    }

    // close the sender
    await sender.CloseAsync();
}
catch (Exception e)
{
    Console.WriteLine(e);
    Debugger.Break();
}
finally
{
    Console.WriteLine("done!");
    Console.ReadKey();
}