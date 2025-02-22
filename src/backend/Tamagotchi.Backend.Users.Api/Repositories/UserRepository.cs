using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Repositories;
using Tamagotchi.Backend.Users.Api.Entities;

namespace Tamagotchi.Backend.Users.Api.Repositories;

public class UserRepository : CosmosDbRepository<UserEntity>, IUserRepository
{
    private readonly ISuperLogger<CosmosDbRepository<UserEntity>> _log;

    public UserRepository(
        CosmosDbFactory cosmosDbFactory,
        ISuperLogger<CosmosDbRepository<UserEntity>> logger
    )
        : base(cosmosDbFactory, logger)
    {
        _log = logger;
    }

    public async Task<CosmosDbResponse<UserEntity?>> GetUserByEmailAsync(
        string email,
        string transactionId
    )
    {
        var query = "SELECT * FROM c WHERE c.email = @Email";
        var parameters = new Dictionary<string, object> { { "@Email", email } };

        var userResponse = await QueryOneAsync(query, parameters, transactionId);
        var user = userResponse;
        return new CosmosDbResponse<UserEntity?>
        {
            IsSuccess = userResponse.IsSuccess,
            Entity = userResponse.Entity,
            ResponseCode = userResponse.ResponseCode,
            Exception = userResponse.Exception,
        };
    }

    public async Task<CosmosDbResponse<UserEntity?>> GetUserByUsernameAsync(
        string username,
        string transactionId
    )
    {
        var query = "SELECT * FROM c WHERE c.username = @Username";
        var parameters = new Dictionary<string, object> { { "@Username", username } };

        var userResponse = await QueryOneAsync(query, parameters, transactionId);
        var user = userResponse;
        return new CosmosDbResponse<UserEntity?>
        {
            IsSuccess = userResponse.IsSuccess,
            Entity = userResponse.Entity,
            ResponseCode = userResponse.ResponseCode,
            Exception = userResponse.Exception,
        };
    }
}
