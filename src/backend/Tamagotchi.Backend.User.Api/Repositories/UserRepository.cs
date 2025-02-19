using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Repositories;
using Tamagotchi.Backend.User.Api.Entities;

namespace Tamagotchi.Backend.User.Api.Repositories;

public class UserRepository : CosmosDbRepository<UserEntity>, IUserRepository
{
    private readonly ISuperLogger<CosmosDbRepository<UserEntity>> _log;

    public UserRepository(ICosmosDbFactory cosmosDbFactory, ISuperLogger<CosmosDbRepository<UserEntity>> logger) : base(cosmosDbFactory, logger)
    {
        _log = logger;
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email, string transactionId)
    {
        try
        {
            var query = "SELECT * FROM c WHERE c.email = @Email";
            var parameters = new Dictionary<string, object> { { "@Email", email } };

            var users = await QueryAsync(query, parameters, transactionId);
            return users.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"Error occurred while fetching user with email {email}.",
                transactionId
            );
            throw;
        }
    }

    public async Task<UserEntity?> GetUserByUsernameAsync(string username, string transactionId)
    {

        try
        {
            var query = "SELECT * FROM c WHERE c.username = @Username";
            var parameters = new Dictionary<string, object> { { "@Username", username } };

            var users = await QueryAsync(query, parameters, transactionId);
            return users.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"Error occurred while fetching user with username {username}.",
                transactionId
            );
            throw;
        }
    }
}
