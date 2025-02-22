using System.Net;
using Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos;
using Tamagotchi.Backend.SharedLibrary.Entities;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Utilities;

namespace Tamagotchi.Backend.SharedLibrary.Repositories;

public class CosmosDbRepository<T> : IDatabaseRepository<T>
    where T : CosmosBaseEntity
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

    public async Task<CosmosDbResponse<List<T>>> GetAllAsync(string transactionId)
    {
        try
        {
            _log.LogInformation($"Fetching all items from Cosmos DB.", transactionId);
            var queryIterator = _container.GetItemQueryIterator<T>(
                new QueryDefinition("SELECT * FROM c")
            );
            var results = new List<T>();

            while (queryIterator.HasMoreResults)
            {
                FeedResponse<T> response = await queryIterator.ReadNextAsync();
                results.AddRange(response);
            }

            _log.LogInformation($"Fetched {results.Count} items from Cosmos DB.", transactionId);
            return new CosmosDbResponse<List<T>>
            {
                IsSuccess = true,
                Entity = results,
                ResponseCode = 200,
            };
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogWarning($"Rate limit exceeded while fetching all items.", transactionId);

            var errorResponse = new CosmosDbResponse<List<T>>()
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 429,
            };

            return errorResponse;
        }
        catch (CosmosException ex)
        {
            _log.LogError(ex, $"Cosmos DB error fetching all items: {ex.Message}", transactionId);
            var errorResponse = new CosmosDbResponse<List<T>>()
            {
                IsSuccess = false,
                ResponseCode = 500,
                Exception = ex,
            };
            return errorResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error fetching all items: {ex.Message}", transactionId);
            return new CosmosDbResponse<List<T>>
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 500,
            };
        }
    }

    public async Task<CosmosDbResponse<T>> GetOneByIdAsync(
        string id,
        string partitionKey,
        string transactionId
    )
    {
        try
        {
            _log.LogInformation($"Item with ID {id} found.", transactionId);
            var entityResponse = await _container.ReadItemAsync<T>(
                id,
                new PartitionKey(partitionKey)
            );

            var response = new CosmosDbResponse<T>() { 
                IsSuccess = true, 
                ResponseCode = 200,
                Entity = entityResponse };
            return response;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _log.LogInformation($"Item with ID {id} not found.", transactionId);

            var response = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 404,
            };
            return response;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogWarning($"Rate limit exceeded while getting item with ID {id}.", transactionId);

            var errorResponse = new CosmosDbResponse<T>() { IsSuccess = false, ResponseCode = 429 };

            return errorResponse;
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error while fetching item with ID {id}: {ex.Message}",
                transactionId
            );
            var errorResponse = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                ResponseCode = 500,
                Exception = ex,
            };
            return errorResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"Error occurred while fetching item with ID {id}: {ex.Message}",
                transactionId
            );

            var response = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                Entity = null,
                ResponseCode = 500,
                Exception = ex,
            };

            return response;
        }
    }

    public async Task<CosmosDbResponse<List<T>>> QueryAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    )
    {
        try
        {
            _log.LogInformation($"Executing query: {query}", transactionId);
            var queryDefinition = new QueryDefinition(query);
            foreach (var param in parameters)
            {
                queryDefinition.WithParameter(param.Key, param.Value);
            }

            var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
            var results = new List<T>();

            while (queryIterator.HasMoreResults)
            {
                FeedResponse<T> response = await queryIterator.ReadNextAsync();
                results.AddRange(response);
            }

            _log.LogInformation($"Query executed. Retrieved {results.Count} items.", transactionId);
            return new CosmosDbResponse<List<T>>
            {
                IsSuccess = true,
                Entity = results,
                ResponseCode = 200,
            };
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogWarning($"Rate limit exceeded while fetching all items.", transactionId);

            var errorResponse = new CosmosDbResponse<List<T>>()
            {
                IsSuccess = false,
                ResponseCode = 429,
                Exception = ex,
            };

            return errorResponse;
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error while executing query: {ex.Message}",
                transactionId
            );
            var errorResponse = new CosmosDbResponse<List<T>>()
            {
                IsSuccess = false,
                ResponseCode = 500,
                Exception = ex,
            };
            return errorResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error executing query.", transactionId);
            return new CosmosDbResponse<List<T>>
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 500,
            };
        }
    }

    public async Task<CosmosDbResponse<T>> QueryOneAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    )
    {
        try
        {
            _log.LogInformation($"Executing query: {query}", transactionId);
            var queryDefinition = new QueryDefinition(query);
            foreach (var param in parameters)
            {
                queryDefinition.WithParameter(param.Key, param.Value);
            }

            var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
            var results = new List<T>();

            while (queryIterator.HasMoreResults)
            {
                FeedResponse<T> response = await queryIterator.ReadNextAsync();
                results.AddRange(response);
            }

            if (results.FirstOrDefault() == null)
            {
                _log.LogInformation($"Query executed. Item not found.", transactionId);
                return new CosmosDbResponse<T>
                {
                    IsSuccess = false,
                    ResponseCode = 404
                };
            }

            _log.LogInformation($"Query executed. Retrieved item.", transactionId);
            return new CosmosDbResponse<T>
            {
                IsSuccess = true,
                Entity = results.FirstOrDefault(),
                ResponseCode = 200,
            };
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogWarning($"Rate limit exceeded while executing query.", transactionId);

            var errorResponse = new CosmosDbResponse<T>() { IsSuccess = false, ResponseCode = 429 };

            return errorResponse;
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error while executing query: {ex.Message}",
                transactionId
            );
            var errorResponse = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                ResponseCode = 500,
                Exception = ex,
            };
            return errorResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error executing query.", transactionId);
            return new CosmosDbResponse<T>
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 500,
            };
        }
    }

    public async Task<CosmosDbResponse<T>> AddAsync(T entity, string transactionId)
    {
        try
        {
            _log.LogInformation($"Adding new item.", transactionId);
            var response = await _container.CreateItemAsync(entity);
            _log.LogInformation($"Item added successfully.", transactionId);

            return new CosmosDbResponse<T>
            {
                IsSuccess = true,
                Entity = response.Resource,
                ResponseCode = 201,
            };
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogWarning($"Rate limit exceeded while adding item.", transactionId);

            var errorResponse = new CosmosDbResponse<T>() { IsSuccess = false, Exception = ex, ResponseCode = 429 };

            return errorResponse;
        }
        catch (CosmosException ex)
        {
            _log.LogError(ex, $"Cosmos DB error while adding item.: {ex.Message}", transactionId);
            var errorResponse = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 500,
            };
            return errorResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error adding item.", transactionId);
            return new CosmosDbResponse<T>
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 500,
            };
        }
    }

    public async Task<CosmosDbResponse<T>> UpdateAsync(
        string id,
        string partitionKey,
        T entity,
        string transactionId
    )
    {
        try
        {
            _log.LogInformation($"Updating item with ID {id}.", transactionId);
            var response = await _container.ReplaceItemAsync(
                entity,
                id,
                new PartitionKey(partitionKey)
            );
            _log.LogInformation($"Item updated successfully.", transactionId);

            return new CosmosDbResponse<T>
            {
                IsSuccess = true,
                Entity = response.Resource,
                ResponseCode = 200,
            };
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogWarning(
                $"Rate limit exceeded while updating item with ID {id}.",
                transactionId
            );

            var errorResponse = new CosmosDbResponse<T>() { IsSuccess = false, Exception = ex, ResponseCode = 429 };

            return errorResponse;
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error while updating item with ID {id}: {ex.Message}",
                transactionId
            );
            var errorResponse = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 500,
            };
            return errorResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error updating item with ID {id}.", transactionId);
            return new CosmosDbResponse<T>
            {
                IsSuccess = false,
                Exception = ex,
                ResponseCode = 500,
            };
        }
    }

    public async Task<CosmosDbResponse<T>> DeleteAsync(
        string id,
        string partitionKey,
        string transactionId
    )
    {
        try
        {
            var deleteResponse = await _container.DeleteItemAsync<T>(
                id,
                new PartitionKey(partitionKey)
            );

            var response = new CosmosDbResponse<T>() { IsSuccess = true, ResponseCode = 204 };
            return response;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _log.LogWarning($"Item with ID {id} not found for deletion.", transactionId);

            var errorResponse = new CosmosDbResponse<T>() { IsSuccess = false, Exception = ex, ResponseCode = 404 };

            return errorResponse;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogWarning(
                $"Rate limit exceeded while deleting item with ID {id}.",
                transactionId
            );

            var errorResponse = new CosmosDbResponse<T>() { IsSuccess = false, Exception = ex, ResponseCode = 429 };

            return errorResponse;
        }
        catch (CosmosException ex)
        {
            _log.LogError(
                ex,
                $"Cosmos DB error while deleting item with ID {id}: {ex.Message}",
                transactionId
            );
            var errorResponse = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                ResponseCode = 500,
                Exception = ex,
            };
            return errorResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error occurred while fetching items: {ex.Message}", transactionId);

            var errorResponse = new CosmosDbResponse<T>()
            {
                IsSuccess = false,
                ResponseCode = 500,
                Exception = ex,
            };
            return errorResponse;
        }
    }
}
