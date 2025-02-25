using Azure.Messaging.ServiceBus;
using Azure.Identity;
using System;

namespace Tamagotchi.Backend.SharedLibrary.Factories
{
    /// <summary>
    /// Factory class for creating and managing instances of <see cref="ServiceBusClient"/>.
    /// Supports authentication via Managed Identity, Shared Access Signature (SAS), and SAS with Entity Path.
    /// </summary>
    public class ServiceBusFactory : IServiceBusFactory
    {
        private readonly ServiceBusClient _serviceBusClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusFactory"/> class using Managed Identity authentication.
        /// </summary>
        /// <param name="sbusNamespace">The fully qualified namespace of the Azure Service Bus.</param>
        /// <exception cref="ArgumentException">Thrown when the namespace is null or empty.</exception>
        public ServiceBusFactory(string sbusNamespace)
        {
            if (string.IsNullOrWhiteSpace(sbusNamespace))
                throw new ArgumentException("The fully qualified namespace of the Azure Service Bus is required.", nameof(sbusNamespace));

            // Authenticate using Managed Identity (DefaultAzureCredential)
            _serviceBusClient = new ServiceBusClient(sbusNamespace, new DefaultAzureCredential());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusFactory"/> class using Shared Access Signature (SAS) authentication.
        /// </summary>
        /// <param name="sbusNamespace">The fully qualified namespace of the Azure Service Bus.</param>
        /// <param name="sasPolicyName">The name of the SAS policy.</param>
        /// <param name="sasKey">The SAS key associated with the policy.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the parameters (namespace, SAS policy name, SAS key) are null or empty.
        /// </exception>
        public ServiceBusFactory(string sbusNamespace, string sasPolicyName, string sasKey)
        {
            if (string.IsNullOrWhiteSpace(sbusNamespace))
                throw new ArgumentException("The fully qualified namespace of the Azure Service Bus is required.", nameof(sbusNamespace));

            if (string.IsNullOrWhiteSpace(sasPolicyName))
                throw new ArgumentException("ServiceBus SAS Policy Name is required.", nameof(sasPolicyName));

            if (string.IsNullOrWhiteSpace(sasKey))
                throw new ArgumentException("ServiceBus SAS Key is required.", nameof(sasKey));

            // Construct Service Bus connection string using SAS
            var connectionString = $"Endpoint=sb://{sbusNamespace}/;SharedAccessKeyName={sasPolicyName};SharedAccessKey={sasKey}";
            _serviceBusClient = new ServiceBusClient(connectionString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusFactory"/> class using Shared Access Signature (SAS) authentication 
        /// with a specified entity path (Queue or Topic).
        /// </summary>
        /// <param name="sbusNamespace">The fully qualified namespace of the Azure Service Bus.</param>
        /// <param name="sasPolicyName">The name of the SAS policy.</param>
        /// <param name="sasKey">The SAS key associated with the policy.</param>
        /// <param name="entityPath">The specific queue or topic to connect to.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the parameters (namespace, SAS policy name, SAS key, entity path) are null or empty.
        /// </exception>
        public ServiceBusFactory(string sbusNamespace, string sasPolicyName, string sasKey, string entityPath)
        {
            if (string.IsNullOrWhiteSpace(sbusNamespace))
                throw new ArgumentException("The fully qualified namespace of the Azure Service Bus is required.", nameof(sbusNamespace));

            if (string.IsNullOrWhiteSpace(sasPolicyName))
                throw new ArgumentException("ServiceBus SAS Policy Name is required.", nameof(sasPolicyName));

            if (string.IsNullOrWhiteSpace(sasKey))
                throw new ArgumentException("ServiceBus SAS Key is required.", nameof(sasKey));

            if (string.IsNullOrWhiteSpace(entityPath))
                throw new ArgumentException("ServiceBus Entity Path is required.", nameof(entityPath));

            // Construct Service Bus connection string with Entity Path
            var connectionString = $"Endpoint=sb://{sbusNamespace}/;SharedAccessKeyName={sasPolicyName};SharedAccessKey={sasKey};EntityPath={entityPath}";
            _serviceBusClient = new ServiceBusClient(connectionString);
        }

        /// <summary>
        /// Gets an instance of <see cref="ServiceBusClient"/> for interacting with Azure Service Bus.
        /// </summary>
        /// <returns>A configured <see cref="ServiceBusClient"/> instance.</returns>
        public ServiceBusClient GetClient()
        {
            return _serviceBusClient;
        }
    }
}
