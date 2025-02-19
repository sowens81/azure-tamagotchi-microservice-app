
using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Backend.SharedLibrary.Dto;

namespace Tamagotchi.Backend.User.Api.Services;

public interface IUserService
{
    Task<IActionResult> CreateUserAsync(UserRegistrationRequestDto userRegistrationObject, string transactionId);
    Task<UserResponseDto> GetUserByIdAsync(string userId);

    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

    Task<UserResponseDto> GetUserByUsernameAsync(string username);

    Task<UserResponseDto> GetUserByEmailAsync(string email);

    Task<bool> DeleteUserByIdAsync(string userId);

    Task<UserResponseDto> UpdateUserAsync(UserUpdateRequestDto userUpdateObject);

}