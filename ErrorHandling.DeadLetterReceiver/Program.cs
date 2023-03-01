using Azure.Messaging.ServiceBus;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var queueName = "dl-queue";

var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

var processorOptions = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 1,
    AutoCompleteMessages = false,
    SubQueue = SubQueue.DeadLetter
};
var processor = client.CreateProcessor(queueName, processorOptions);
Console.WriteLine($"Dead-letter queue path: {processor.EntityPath}");

processor.ProcessMessageAsync += ProcessMessageAsync;
processor.ProcessErrorAsync += ProcessErrorAsync;

await processor.StartProcessingAsync();

Console.WriteLine("Received dead-letter messages:");
Console.ReadLine();

await processor.StopProcessingAsync();
await processor.CloseAsync();

async Task ProcessMessageAsync(ProcessMessageEventArgs args)
{
    Console.WriteLine("\t Received dead-letter message");
    Console.WriteLine($"\t\t Dead-letter reason: {args.Message.DeadLetterReason}");
    Console.WriteLine($"\t\t Dead-letter error description: {args.Message.DeadLetterErrorDescription}");

    await args.CompleteMessageAsync(args.Message);
    Console.WriteLine();
}

async Task ProcessErrorAsync(ProcessErrorEventArgs args)
{

}