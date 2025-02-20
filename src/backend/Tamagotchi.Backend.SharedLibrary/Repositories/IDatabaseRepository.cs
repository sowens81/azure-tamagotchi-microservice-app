using Tamagotchi.Backend.SharedLibrary.Entities;
using Tamagotchi.Backend.SharedLibrary.Models;

namespace Tamagotchi.Backend.SharedLibrary.Repositories;

public interface IDatabaseRepository<T>
    where T : class
{
    Task<CosmosDbResponse<T>> GetOneByIdAsync(string id, string partitionKey, string transactionId);

    Task<CosmosDbResponse<List<T>>> GetAllAsync(string transactionId);
    Task<CosmosDbResponse<List<T>>> QueryAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    );
    Task<CosmosDbResponse<T>> QueryOneAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    );
    Task<CosmosDbResponse<T>> AddAsync(T entity, string transactionId);
    Task<CosmosDbResponse<T>> UpdateAsync(
        string id,
        string partitionKey,
        T entity,
        string transactionId
    );
    Task<CosmosDbResponse<T>> DeleteAsync(string id, string partitionKey, string transactionId);
}
