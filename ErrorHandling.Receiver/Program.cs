using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using ErrorHandling.Common;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var queueName = "dl-queue";
var forwardQueue = "forward-dl-queue";

var adminClient = new ServiceBusAdministrationClient(connectionString);
if (!await adminClient.QueueExistsAsync(queueName))
{
    await adminClient.CreateQueueAsync(new CreateQueueOptions(queueName)
    {
        LockDuration = TimeSpan.FromSeconds(5)
    });
}

if (!await adminClient.QueueExistsAsync(forwardQueue))
{
    await adminClient.CreateQueueAsync(forwardQueue);
}

var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

var processorOptions = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 1,
    AutoCompleteMessages = false
};

var processor = client.CreateProcessor(queueName, processorOptions);
processor.ProcessMessageAsync += ProcessMessageAsync;
processor.ProcessErrorAsync += ProcessErrorAsync;

await processor.StartProcessingAsync();

Console.ReadKey();

await processor.StopProcessingAsync();
await processor.CloseAsync();

async Task ProcessMessageAsync(ProcessMessageEventArgs args)
{
    var message = args.Message;
    var contentType = message.ContentType;
    Console.WriteLine($"Received message of type: {contentType}");

    switch (contentType)
    {
        case "text/plain":
            Console.WriteLine($"Message content: {message.Body}, deliveryCount = {message.DeliveryCount}");

            try
            {
                var forwardingMessage = new ServiceBusMessage();
                var forwardSender = client.CreateSender(forwardQueue);
                await forwardSender.SendMessageAsync(forwardingMessage);

                await args.CompleteMessageAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception occurred! {0}", exception);

                //await args.AbandonMessageAsync(message);

                if (message.DeliveryCount > 5)
                {
                    await args.DeadLetterMessageAsync(message, exception.Message, exception.ToString());
                }
                else
                {
                    //await args.AbandonMessageAsync(message);
                }
            }
            break;
        case "application/json":
            try
            {
                Console.WriteLine(
                    $"Message content: {JsonSerializer.Deserialize<Person>(message.Body)}, deliveryCount = {message.DeliveryCount}");
                await args.CompleteMessageAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {0}", exception);

                await args.DeadLetterMessageAsync(message, exception.Message, exception.ToString());
            }
            break;
        default:
            Console.WriteLine($"Received unknown message: {contentType}");
            await args.DeadLetterMessageAsync(message, "Unknown message", $"Invalid message content type: {contentType}");
            break;
    }
}

async Task ProcessErrorAsync(ProcessErrorEventArgs args)
{

}