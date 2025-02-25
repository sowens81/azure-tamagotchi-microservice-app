using System.Threading.Tasks;

namespace Tamagotchi.Backend.Users.Api.Services
{
    /// <summary>
    /// Interface for sending messages to Azure Service Bus.
    /// </summary>
    public interface IServiceBusService
    {
        /// <summary>
        /// Sends a message to Azure Service Bus.
        /// </summary>
        /// <typeparam name="T">The type of the message being sent.</typeparam>
        /// <param name="message">The message object to send.</param>
        /// <param name="transactionId">A unique transaction ID for tracking.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendMessageAsync<T>(string messageType, T message, string transactionId);
    }
}
