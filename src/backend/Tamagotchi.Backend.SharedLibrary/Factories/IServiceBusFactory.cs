using Azure.Messaging.ServiceBus;

namespace Tamagotchi.Backend.SharedLibrary.Factories;

public interface IServiceBusFactory
{
    ServiceBusClient GetClient();
}
