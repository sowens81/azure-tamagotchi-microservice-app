using Container = Microsoft.Azure.Cosmos.Container;

namespace Tamagotchi.Backend.SharedLibrary.Factories;

public interface ICosmosDbFactory
{
    Container GetContainer();
}
