namespace Tamagotchi.Backend.Pets.Worker.Services;

public interface IServiceBusReceiverService
{
    /// <summary>
    /// Starts receiving messages from the Azure Service Bus queue or topic.
    /// </summary>
    /// <param name="queueOrTopicName">The name of the queue or topic.</param>
    /// <param name="subscriptionName">The subscription name (for topics, optional for queues).</param>
    Task StartReceivingMessagesAsync(string queueOrTopicName, string? subscriptionName = null);
}
