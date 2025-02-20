using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Backend.User.Api.Dtos;

namespace Tamagotchi.Backend.User.Api.Services;

public interface IUserService
{
    Task<IActionResult> CreateUserAsync(
        UserRegistrationRequestDto userRegistrationObject,
        string transactionId
    );
    Task<IActionResult> GetUserByIdAsync(string userId, string transactionId);

    Task<IActionResult> GetAllUsersAsync(string transactionId);

    Task<IActionResult> GetUserByUsernameAsync(string username, string transactionId);

    Task<IActionResult> GetUserByEmailAsync(string email, string transactionId);

    Task<IActionResult> DeleteUserByIdAsync(string userId, string transactionId);

    Task<IActionResult> UpdateUserAsync(
        string userId,
        UserUpdateRequestDto userUpdateObject,
        string transactionId
    );
}
