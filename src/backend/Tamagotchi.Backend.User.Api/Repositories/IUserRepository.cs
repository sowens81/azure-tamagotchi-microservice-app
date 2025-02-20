using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Repositories;
using Tamagotchi.Backend.User.Api.Entities;

namespace Tamagotchi.Backend.User.Api.Repositories;

public interface IUserRepository : IDatabaseRepository<UserEntity>
{
    Task<CosmosDbResponse<UserEntity?>> GetUserByEmailAsync(string email, string transactionId);

    Task<CosmosDbResponse<UserEntity?>> GetUserByUsernameAsync(
        string username,
        string transactionId
    );
}
