using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using TopicsMessaging.Common;

var connectionString = "Endpoint=sb://simplemessaging-sb-demo-students.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8CssayAWC1m9CO1jbjsS6NpjBUWK3vJA+ASbA3Plyk=";
var topicName = "orderstopic";

var adminClient = new ServiceBusAdministrationClient(connectionString);

// general receiver
await adminClient.CreateSubscriptionAsync(topicName, "allOrdersSubscription");

// usa region only loyal customers receiver
var ruleOptions = new CreateRuleOptions("region", new SqlRuleFilter("region = 'USA' AND loyalty = true"));
var subscriptionOptions = new CreateSubscriptionOptions(topicName, "usaOrderSubscription")
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};
await adminClient.CreateSubscriptionAsync(subscriptionOptions, ruleOptions);

// large orders receiver
ruleOptions = new CreateRuleOptions("items", new SqlRuleFilter("items > 30"));
subscriptionOptions = new CreateSubscriptionOptions(topicName, "largeOrderSubscription")
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};
await adminClient.CreateSubscriptionAsync(subscriptionOptions, ruleOptions);

// correlated uk receivers
ruleOptions = new CreateRuleOptions("region", new CorrelationRuleFilter("UK"));
subscriptionOptions = new CreateSubscriptionOptions(topicName, "ukSubscription")
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};
await adminClient.CreateSubscriptionAsync(subscriptionOptions, ruleOptions);

// correlation subscription for test messages
ruleOptions = new CreateRuleOptions("testMessages", new CorrelationRuleFilter
{
    Subject = "test"
});
subscriptionOptions = new CreateSubscriptionOptions(topicName, "testSubscription")
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};
await adminClient.CreateSubscriptionAsync(subscriptionOptions, ruleOptions);

var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
});

var subscriptions = adminClient.GetSubscriptionsAsync(topicName);
var subsEnumerator = subscriptions.GetAsyncEnumerator();
while (await subsEnumerator.MoveNextAsync())
{
    var sub = subsEnumerator.Current;
    Console.WriteLine($"Processing orders for {sub.TopicName}/{sub.SubscriptionName}");

    var receiver = client.CreateProcessor(sub.TopicName, sub.SubscriptionName);
    receiver.ProcessMessageAsync += MessageHandler;
    receiver.ProcessErrorAsync += ErrorHandler;

    await receiver.StartProcessingAsync();
    Console.ReadKey();
    await receiver.StopProcessingAsync();
    Console.WriteLine();
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
    Console.WriteLine("\t" + JsonSerializer.Deserialize<Order>(args.Message.Body));
    await args.CompleteMessageAsync(args.Message);
}

async Task ErrorHandler(ProcessErrorEventArgs args)
{
    
}