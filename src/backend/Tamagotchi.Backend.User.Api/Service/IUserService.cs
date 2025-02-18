
using Tamagotchi.Backend.SharedLibrary.Dto;

namespace Tamagotchi.Backend.User.Api.Service;

public interface IUserService
{
    Task<UserRegistrationResponseDto> RegisterUserAsync(UserRegistrationRequestDto userRegistrationObject);
    Task<UserResponseDto> GetUserByIdAsync(string userId);

    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

    Task<UserResponseDto> GetUserByUsernameAsync(string username);

    Task<UserResponseDto> GetUserByEmailAsync(string email);

    Task<bool> DeleteUserByIdAsync(string userId);

    Task<UserResponseDto> UpdateUserAsync(UserUpdateRequestDto userUpdateObject);

}