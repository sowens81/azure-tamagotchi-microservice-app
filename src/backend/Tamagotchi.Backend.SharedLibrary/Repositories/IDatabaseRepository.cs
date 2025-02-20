using Tamagotchi.Backend.SharedLibrary.Entities;
using Tamagotchi.Backend.SharedLibrary.Models;

namespace Tamagotchi.Backend.SharedLibrary.Repositories;

public interface IDatabaseRepository<T> 
    where T : class
{
    Task<CosmosDbResponse<T>> GetOneByIdAsync(string id, string partitionKey, string transactionId);
    Task<IEnumerable<T>> GetAllAsync(string transactionId);
    Task<IEnumerable<T>> QueryAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    );
    Task<T?> QueryFirstOrDefaultAsync(
        string query,
        IDictionary<string, object> parameters,
        string transactionId
    );
    Task<T> AddAsync(T entity, string transactionId);
    Task UpdateAsync(string id, string partitionKey, T entity, string transactionId);
    Task<CosmosDbResponse<T>> DeleteAsync(string id, string partitionKey, string transactionId);
    Task UpdateByFieldAsync(
        string fieldName,
        object fieldValue,
        string partitionKey,
        T updatedEntity,
        string transactionId
    );
    Task DeleteByFieldAsync(
        string fieldName,
        object fieldValue,
        string partitionKey,
        string transactionId
    );
}
