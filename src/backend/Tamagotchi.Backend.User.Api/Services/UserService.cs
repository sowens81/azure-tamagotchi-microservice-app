using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Web.Mvc;
using Tamagotchi.Backend.SharedLibrary.Dto;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Security;
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

    public async Task<IActionResult> CreateUserAsync(UserRegistrationRequestDto userRegistrationObject, string transactionId)
    {
        var validateUsername = await _userRepository.GetUserByUsernameAsync(userRegistrationObject.Username, transactionId);
        if (null != validateUsername)
        {
            _log.LogWarning($"User Registation Request: User account for username {userRegistrationObject.Username} already exists", transactionId);
            return new BadRequestObjectResult(new { message = "Bad Request, Username already registered." });
        }

        var validateEmail = await _userRepository.GetUserByUsernameAsync(userRegistrationObject.Username, transactionId);
        if (null != validateEmail)
        {
            _log.LogWarning($"User Registation Request: User account for email address {userRegistrationObject.Email} already exists", transactionId);
            return new BadRequestObjectResult(new { message = "Bad Request, Email address already registered." });
        }


        var userEntity = new UserEntity
        {
            Username = userRegistrationObject.Username,
            Email = userRegistrationObject.Email,
            PasswordHash = _passwordHasher.HashPassword(userRegistrationObject.Password),
            FirstName = userRegistrationObject.FirstName,
            LastName = userRegistrationObject.LastName,
        };

        // Save the user to the database, send confirmation email, etc.
        var addUserResponse = await _userRepository.AddAsync(userEntity, transactionId);

        var userRegistrationResponse = new UserRegistrationResponseDto()
        {
            Message = "Successfully registered new user.",
            UserId = addUserResponse.UserId,
            Username = addUserResponse.Username,
            CreatedAt = addUserResponse.CreatedAt
        };

        _log.LogInformation($"User Registation Request: User account created for {userRegistrationObject.Username}", transactionId);
        return new OkObjectResult(userRegistrationResponse);
    }

    public async Task<bool> DeleteUserByIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<UserResponseDto> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<UserResponseDto> GetUserByIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<UserResponseDto> GetUserByUsernameAsync(string username)
    {
        throw new NotImplementedException();
    }

    public async Task<UserResponseDto> UpdateUserAsync(UserUpdateRequestDto userUpdateObject)
    {
        throw new NotImplementedException();
    }
}
