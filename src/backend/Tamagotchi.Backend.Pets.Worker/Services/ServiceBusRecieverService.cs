using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;

namespace Tamagotchi.Backend.Pets.Worker.Services;

public class ServiceBusRecieverService : IServiceBusReceiverService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ISuperLogger<ServiceBusRecieverService> _log;
    private ServiceBusProcessor? _processor;

    /// <summary>
    /// Initializes a new instance of the Service Bus Receiver Service.
    /// </summary>
    public ServiceBusRecieverService(IServiceBusFactory serviceBusFactory, ISuperLogger<ServiceBusRecieverService> log)
    {
        _serviceBusClient = serviceBusFactory.GetClient();
        _log = log;
    }

    /// <summary>
    /// Starts receiving messages from a Service Bus queue or topic subscription.
    /// </summary>
    public async Task StartReceivingMessagesAsync(string queueOrTopicName, string? subscriptionName = null)
    {
        if (_processor != null)
        {
            _log.LogWarning($"Message processing is already running for '{queueOrTopicName}'.", Guid.NewGuid().ToString());
            return;
        }

        // Determine whether it’s a queue or a topic with a subscription
        _processor = string.IsNullOrEmpty(subscriptionName)
            ? _serviceBusClient.CreateProcessor(queueOrTopicName, new ServiceBusProcessorOptions())
            : _serviceBusClient.CreateProcessor(queueOrTopicName, subscriptionName, new ServiceBusProcessorOptions());

        // Attach message handler
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        _log.LogInformation($"Starting message processing for '{queueOrTopicName}'...", Guid.NewGuid().ToString());

        await _processor.StartProcessingAsync();
    }

    /// <summary>
    /// Handles received messages.
    /// </summary>
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            string body = Encoding.UTF8.GetString(args.Message.Body);
            var messageType = args.Message.Subject;

            if ( messageType == "USER_REGISTER")
            {
                
            }

            if (messageType == "USER_UNREGISTER")

            await Task.Delay(500);

            // Complete the message to remove it from the queue
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error processing message");
        }
    }

    /// <summary>
    /// Handles errors during message processing.
    /// </summary>
    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _log.LogError(args.Exception, $"Error in Service Bus processing: {args.Exception.Message}");
        return Task.CompletedTask;
    }
}
