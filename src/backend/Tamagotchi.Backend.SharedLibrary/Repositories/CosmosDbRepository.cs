using Microsoft.Azure.Cosmos;
using System.Net;
using Tamagotchi.Backend.SharedLibrary.Entities;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Utilities;

namespace Tamagotchi.Backend.SharedLibrary.Repositories;

public class CosmosDbRepository<T> : IDatabaseRepository<T> where T : CosmosBaseEntity
{
    private readonly Container _container;
    private readonly ISuperLogger<CosmosDbRepository<T>> _log;

    public CosmosDbRepository(
        ICosmosDbFactory cosmosDbFactory,
        ISuperLogger<CosmosDbRepository<T>> logger
    )
    {
        _container = cosmosDbFactory.GetContainer();
        _log = logger;
    }

    public async Task<T?> GetOneByIdAsync(string id, string partitionKey, string transactionId)
    {
        try
        {
            _log.LogInformation($"Item with ID {id} found.", transactionId);
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _log.LogInformation($"Item with ID {id} not found.", transactionId);
            return null;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error occurred while fetching item with ID {id}.", transactionId);
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(string transactionId)
    {
        try
        {
            // Define a default query to fetch all items
            var queryIterator = _container.GetItemQueryIterator<T>(
                new QueryDefinition("SELECT * FROM c")
            );
            var results = new List<T>();

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            if (results.Count == 0)
            {
                _log.LogInformation($"No items found.", transactionId);
                return results;
            }

            _log.LogInformation($"Items found.", transactionId);
            return results;


        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error occurred while fetching all items.", transactionId);
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    )
    {
        try
        {
            var queryDefinition = new QueryDefinition(query);

            // Add parameters to the query
            foreach (var parameter in parameters)
            {
                queryDefinition.WithParameter(parameter.Key, parameter.Value);
            }

            var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

            var results = new List<T>();
            while (queryIterator.HasMoreResults)
            {
                try
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }
                catch (CosmosException ex)
                    when (ex.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                {
                    _log.LogWarning(
                        $"Query exceeded entity size limit. Query: {query}",
                        transactionId
                    );
                    throw; // Re-throw to allow calling code to handle appropriately
                }
            }

            if (results.Count == 0)
            {
                _log.LogInformation($"No items found.", transactionId);
                return results;
            }
            _log.LogInformation($"Items found.", transactionId);
            return results;
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error occurred while executing query: {query}",
                transactionId
            );
            throw; // Re-throw to allow calling code to handle appropriately
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"An unexpected error occurred while executing query: {query}",
                transactionId
            );
            throw;
        }
    }

    public async Task<T?> QueryFirstOrDefaultAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    )
    {
        // Reuse your existing method to get all matching results
        var results = await QueryAsync(query, parameters, transactionId);

        // Return only the first record or null if none
        return results.FirstOrDefault();
    }

    public async Task<T> AddAsync(T entity, string transactionId)
    {
        try
        {
            entity.UserId = IdGenerator.GenerateShortId();
            var response = await _container.CreateItemAsync(entity);
            return response;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            _log.LogWarning($"Item already exists.", transactionId);
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error occurred while adding item.", transactionId);
            throw;
        }
    }

    public async Task UpdateAsync(string id, string partitionKey, T entity, string transactionId)
    {
        try
        {
            await _container.ReplaceItemAsync(entity, id, new PartitionKey(partitionKey));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _log.LogWarning($"Item with ID {id} not found for update.", transactionId);
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error occurred while updating item.", transactionId);
            throw;
        }
    }

    public async Task DeleteAsync(string id, string partitionKey, string transactionId)
    {
        try
        {
            await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _log.LogWarning($"Item with ID {id} not found for deletion.", transactionId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error occurred while fetching items.", transactionId);
            throw;
        }
    }

    public async Task UpdateByFieldAsync(
        string fieldName,
        object fieldValue,
        string partitionKey,
        T updatedEntity,
        string transactionId
    )
    {
        try
        {
            // Query for the existing entity by the field
            var query =
                $"SELECT * FROM c WHERE c.{fieldName} = @FieldValue AND c.partitionKey = @PartitionKey";
            var parameters = new Dictionary<string, object>
            {
                { "@FieldValue", fieldValue },
                { "@PartitionKey", partitionKey },
            };

            var existingEntities = await QueryAsync(query, parameters, transactionId);

            var entityToUpdate = existingEntities.FirstOrDefault();
            if (entityToUpdate == null)
            {
                _log.LogWarning(
                    $"No entity found with {fieldName} = {fieldValue} in partition {partitionKey}.",
                    transactionId
                );
                return;
            }

            // Extract the id from the existing entity (assuming entities have an 'id' property)
            var idProperty = typeof(T).GetProperty("id");
            if (idProperty == null)
            {
                throw new InvalidOperationException("Entity does not contain an 'id' property.");
            }

            var id = idProperty.GetValue(entityToUpdate)?.ToString();
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("Entity 'id' is null or empty.");
            }

            // Perform the update
            await UpdateAsync(id, partitionKey, updatedEntity, transactionId);
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error occurred while updating entity with {fieldName} = {fieldValue}.",
                transactionId
            );
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"An unexpected error occurred while updating entity with {fieldName} = {fieldValue}.",
                transactionId
            );
            throw;
        }
    }

    public async Task DeleteByFieldAsync(
        string fieldName,
        object fieldValue,
        string partitionKey,
        string transactionId
    )
    {
        try
        {
            // Query for the entity by the field
            var query =
                $"SELECT * FROM c WHERE c.{fieldName} = @FieldValue AND c.partitionKey = @PartitionKey";
            var parameters = new Dictionary<string, object>
            {
                { "@FieldValue", fieldValue },
                { "@PartitionKey", partitionKey },
            };

            var existingEntities = await QueryAsync(query, parameters, transactionId);

            var entityToDelete = existingEntities.FirstOrDefault();
            if (entityToDelete == null)
            {
                _log.LogWarning(
                    $"No entity found with {fieldName} = {fieldValue} in partition {partitionKey}.",
                    transactionId
                );
                return;
            }

            // Extract the id from the existing entity (assuming entities have an 'id' property)
            var idProperty = typeof(T).GetProperty("id");
            if (idProperty == null)
            {
                throw new InvalidOperationException("Entity does not contain an 'id' property.");
            }

            var id = idProperty.GetValue(entityToDelete)?.ToString();
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("Entity 'id' is null or empty.");
            }

            // Perform the deletion
            await DeleteAsync(id, partitionKey, transactionId);
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error occurred while deleting entity with {fieldName} = {fieldValue}.",
                transactionId
            );
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"An unexpected error occurred while deleting entity with {fieldName} = {fieldValue}.",
                transactionId
            );
            throw;
        }
    }
}
