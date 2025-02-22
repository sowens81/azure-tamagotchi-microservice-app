using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Repositories;
using Tamagotchi.Backend.Users.Api.Entities;

namespace Tamagotchi.Backend.Users.Api.Repositories;

public interface IUserRepository : IDatabaseRepository<UserEntity>
{
    Task<CosmosDbResponse<UserEntity?>> GetUserByEmailAsync(string email, string transactionId);

    Task<CosmosDbResponse<UserEntity?>> GetUserByUsernameAsync(
        string username,
        string transactionId
    );
}
