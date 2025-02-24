using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tamagotchi.Backend.SharedLibrary.Factories;

namespace Tamagotchi.Backend.SharedLibrary.Extensions
{
    public class CosmosDbHealthCheckExtension : IHealthCheck
    {
        private readonly ICosmosDbFactory _cosmosDbFactory;

        public CosmosDbHealthCheckExtension(ICosmosDbFactory cosmosDbFactory)
        {
            _cosmosDbFactory =
                cosmosDbFactory ?? throw new ArgumentNullException(nameof(cosmosDbFactory));
        }

        // Updated method signature to include HealthCheckContext
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                // Attempt to read from the Cosmos DB container
                var container = _cosmosDbFactory.GetContainer();

                // Simple query to verify that the container is accessible
                var iterator = container.GetItemQueryIterator<dynamic>("SELECT TOP 1 * FROM c");
                var result = await iterator.ReadNextAsync(cancellationToken);

                // If successful, Cosmos DB is available
                return HealthCheckResult.Healthy("Cosmos DB is available");
            }
            catch (CosmosException cosmosException)
            {
                // Cosmos DB related issues
                return HealthCheckResult.Unhealthy(
                    "Cosmos DB is unavailable",
                    exception: cosmosException
                );
            }
            catch (Exception ex)
            {
                // Any other exception
                return HealthCheckResult.Unhealthy("Cosmos DB is unavailable", exception: ex);
            }
        }
    }
}
