using Tamagotchi.Backend.SharedLibrary.Repositories;
using UserEntity = Tamagotchi.Backend.SharedLibrary.Entities.User;

namespace Tamagotchi.Backend.User.Api.Repository;

public interface IUserRepository : IDatabaseRepository<UserEntity>
{
    Task<UserEntity?> GetUserByEmailAsync(string email, string transactionId);

    Task<UserEntity?> GetUserByUsernameAsync(string username, string transactionId);
}
