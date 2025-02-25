using Azure.Messaging.ServiceBus;
using Polly;
using Polly.Retry;
using System.Drawing.Printing;
using System.Text;
using System.Text.Json;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;

namespace Tamagotchi.Backend.Users.Api.Services
{
    public class ServiceBusService : IServiceBusService
    {
        private readonly string _queueName;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ISuperLogger<ServiceBusService> _log;
        private readonly AsyncRetryPolicy _retryPolicy;

        /// <summary>
        /// Initializes the Service Bus Service.
        /// </summary>
        /// <param name="queueName">Queue name to send messages to.</param>
        /// <param name="serviceBusFactory">Factory for creating a Service Bus client.</param>
        /// <param name="log">Logger instance.</param>
        public ServiceBusService(string queueName, IServiceBusFactory serviceBusFactory, ISuperLogger<ServiceBusService> log)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("The queue name must be provided.", nameof(queueName));

            _queueName = queueName;
            _serviceBusClient = serviceBusFactory.GetClient();
            _serviceBusSender = _serviceBusClient.CreateSender(_queueName);
            _log = log;

            // Configure a retry policy using Polly
            _retryPolicy = Policy
                .Handle<ServiceBusException>(ex => ex.Reason == ServiceBusFailureReason.ServiceBusy || ex.Reason == ServiceBusFailureReason.QuotaExceeded)
                .Or<TimeoutException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        string? retryTransactionId = context.ContainsKey("TransactionId") ? context["TransactionId"].ToString() : Guid.NewGuid().ToString();

                        if (retryTransactionId == null)
                        {
                            retryTransactionId = Guid.NewGuid().ToString();
                        }
                        
                        _log.LogWarning($"Retry {retryCount} for sending message to '{_queueName}' due to {exception.Message}. Waiting {timeSpan.TotalSeconds} seconds before next attempt.", retryTransactionId);
 
                    }
                );
        }

        /// <summary>
        /// Sends a message to Azure Service Bus with retry handling.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message object.</param>
        /// <param name="transactionId">Transaction ID for tracking.</param>
        public async Task SendQueueMessageAsync<T>(string messageType, T message, string transactionId)
        {
            try
            {
                // Serialize message
                string messageBody;
                try
                {
                    messageBody = JsonSerializer.Serialize(message);
                }
                catch (JsonException ex)
                {
                    _log.LogError(ex, "Failed to serialize the message.", transactionId);
                    throw new Exception("Invalid message format. Could not serialize message.", ex);
                }

                // Create Service Bus message
                var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
                {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = transactionId,
                    ApplicationProperties = { ["TransactionId"] = transactionId },
                    Subject = messageType
                };

                _log.LogInformation($"Preparing to send message to '{_queueName}'", transactionId);

                var pollyContext = new Context();
                pollyContext["TransactionId"] = transactionId;

                // Execute with retry policy
                await _retryPolicy.ExecuteAsync(async (context) =>
                {
                    string? retryTransactionId = context.ContainsKey("TransactionId") ? context["TransactionId"].ToString() : Guid.NewGuid().ToString();

                    if (retryTransactionId == null)
                    {
                        retryTransactionId = Guid.NewGuid().ToString();
                    }

                    await _serviceBusSender.SendMessageAsync(serviceBusMessage);
                    _log.LogInformation($"Message successfully sent to '{_queueName}'", retryTransactionId);
                }, pollyContext);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                _log.LogError(ex, $"Queue '{_queueName}' does not exist.", transactionId);
                throw new Exception($"Queue '{_queueName}' was not found. Please verify the queue name.", ex);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.QuotaExceeded)
            {
                _log.LogError(ex, "Quota exceeded for Service Bus.", transactionId);
                throw new Exception("Quota exceeded. Please wait before sending more messages.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.LogError(ex, "Access denied to Azure Service Bus.", transactionId);
                throw new Exception("Unauthorized access to Service Bus. Check credentials and permissions.", ex);
            }
            catch (TimeoutException ex)
            {
                _log.LogError(ex, "Service Bus request timed out.", transactionId);
                throw new Exception("Timeout occurred while sending message. Try again later.", ex);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected error occurred while sending message: {ex.Message}", transactionId);
                throw;
            }
        }
    }
}
