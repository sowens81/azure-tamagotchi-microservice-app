using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.User.Api.Dtos;
using Tamagotchi.Backend.User.Api.Entities;
using Tamagotchi.Backend.User.Api.Repositories;

namespace Tamagotchi.Backend.User.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ISuperLogger<UserService> _log;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public UserService(
        IUserRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ISuperLogger<UserService> logger
    )
    {
        _userRepository = repository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _log = logger;
    }

    public async Task<IActionResult> CreateUserAsync(
        UserRegistrationRequestDto userRegistrationRequest,
        string transactionId
    )
    {
        try
        {
            // Validate if the username already exists
            var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(
                userRegistrationRequest.Username,
                transactionId
            );
            if (existingUserByUsername != null)
            {
                _log.LogWarning(
                    "User Registration Request: User account for username {Username} already exists. Transaction ID: {TransactionId}",
                    userRegistrationRequest.Username,
                    transactionId
                );
                return new ConflictObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Conflict: Username already registered.",
                        ErrorCode = "NURUE001",
                    }
                );
            }

            // Validate if the email already exists
            var existingUserByEmail = await _userRepository.GetUserByEmailAsync( // Fixed the method name
                userRegistrationRequest.Email,
                transactionId
            );
            if (existingUserByEmail != null)
            {
                _log.LogWarning(
                    "User Registration Request: User account for email {Email} already exists. Transaction ID: {TransactionId}",
                    userRegistrationRequest.Email,
                    transactionId
                );
                return new ConflictObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Conflict: Email address already registered.",
                        ErrorCode = "NUREE001",
                    }
                );
            }

            // Create new user entity
            var userEntity = new UserEntity
            {
                Username = userRegistrationRequest.Username,
                Email = userRegistrationRequest.Email,
                PasswordHash = _passwordHasher.HashPassword(userRegistrationRequest.Password),
                FirstName = userRegistrationRequest.FirstName,
                LastName = userRegistrationRequest.LastName,
            };

            // Save the user to the database
            var addUserResponse = await _userRepository.AddAsync(userEntity, transactionId);

            var userRegistrationResponse = new UserRegistrationResponseDto()
            {
                UserId = addUserResponse.UserId,
                Username = addUserResponse.Username,
                CreatedAt = addUserResponse.CreatedAt,
            };

            _log.LogInformation(
                "User Registration Request: User account created for {Username}. Transaction ID: {TransactionId}",
                userRegistrationRequest.Username,
                transactionId
            );

            return new CreatedResult(
                $"/users/{addUserResponse.UserId}", // Specify resource location
                new ApiSuccessResponse()
                {
                    Success = true,
                    Message = "Successfully registered new user.",
                    Metadata = userRegistrationResponse
                }
            );
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"User Registration Request: An error occurred - {ex.Message}.",
                transactionId
            );
            return new ObjectResult(new ApiFailureResponse()
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later.",
                ErrorCode = "NUREE500",
            })
            {
                StatusCode = 500 // Explicitly setting the status code
            };
        }
    }

    public async Task<IActionResult> DeleteUserByIdAsync(string userId, string transactionId)
    {
        try
        {
            var accountExists = await _userRepository.GetOneByIdAsync(userId, userId, transactionId);

            if (null == accountExists)
            {
                _log.LogWarning(
                    $"User Deleten Request: User account {userId} does not exist.",
                    transactionId
                );
                return new NotFoundObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Bad Request, User account does not exist.",
                        ErrorCode = "DUREE0001",
                    }
                );
            }

            var deleted = await _userRepository.DeleteAsync(userId, userId, transactionId);
        }
        catch (Exception)
        {

            throw;
        }
        
    }

    public async Task<IActionResult> GetAllUsersAsync(string transactionId)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> GetUserByEmailAsync(string email, string transactionId)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> GetUserByIdAsync(string userId, string transactionId)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> GetUserByUsernameAsync(string username, string transactionId)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> UpdateUserAsync(
        UserUpdateRequestDto userUpdateRequest,
        string transactionId
    )
    {
        throw new NotImplementedException();
    }
}
