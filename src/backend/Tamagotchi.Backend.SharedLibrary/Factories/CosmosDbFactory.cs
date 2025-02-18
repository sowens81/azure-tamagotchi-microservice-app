using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;

namespace Tamagotchi.Backend.SharedLibrary.Factories;

public class CosmosDbFactory : ICosmosDbFactory
{
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _container;

    public CosmosDbFactory(
        string accountEndpoint,
        string databaseName,
        string containerName
    )
    {
        if (string.IsNullOrWhiteSpace(accountEndpoint))
            throw new ArgumentException(
                "Cosmos DB account endpoint is required.",
                nameof(accountEndpoint)
            );

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("Database name is required.", nameof(databaseName));

        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name is required.", nameof(containerName));

        // Authenticate using Managed Identity or Environment-based credentials
        var credential = new DefaultAzureCredential();

        // Initialize CosmosClient
        _cosmosClient = new CosmosClient(accountEndpoint, credential);

        // Verify resources exist
        _database = _cosmosClient.GetDatabase(databaseName);
        _container = _database.GetContainer(containerName);
    }

    public CosmosDbFactory(
        string accountEndpoint,
        string accountKey,
        string databaseName,
        string containerName
    )
    {
        if (string.IsNullOrWhiteSpace(accountEndpoint))
            throw new ArgumentException(
                "Cosmos DB account endpoint is required.",
                nameof(accountEndpoint)
            );

        if (string.IsNullOrWhiteSpace(accountKey))
            throw new ArgumentException("Cosmos DB account key is required.", nameof(accountKey));

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("Database name is required.", nameof(databaseName));

        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name is required.", nameof(containerName));

        // Initialize CosmosClient using the account endpoint and account key
        _cosmosClient = new CosmosClient(accountEndpoint, accountKey);

        // Verify resources exist
        _database = _cosmosClient.GetDatabase(databaseName);
        _container = _database.GetContainer(containerName);
    }

    public Container GetContainer() => _container;
}
