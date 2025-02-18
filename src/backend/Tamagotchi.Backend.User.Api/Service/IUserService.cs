
namespace Tamagotchi.Backend.User.Api.Service;

public interface IUserService
{
    Task<string UserId> RegisterUserAsync(UserRegistrationDto userRegistrationObject);
    Task<UserDto> GetUserByIdAsync(string userId);

    Task<IEnumerable<UserDto>> GetAllUsersAsync();

    Task<UserDto> GetUserByUsernameAsync(string username);

    Task<UserDto> GetUserByEmailAsync(string email);

    Task<bool> DeleteUserByIdAsync(string userId);

    Task<UserDto> UpdateUserAsync(UserUpdateDto userUpdateObject);

}