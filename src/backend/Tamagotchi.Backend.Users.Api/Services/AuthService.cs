using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Security.Claims;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.Users.Api.Dtos;
using Tamagotchi.Backend.Users.Api.Repositories;
using Tamagotchi.Backend.Users.Api.Utilities;

namespace Tamagotchi.Backend.Users.Api.Services;

public class AuthService : IAuthService
{
    private readonly ISuperLogger<AuthService> _log;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        ISuperLogger<AuthService> logger,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService
        )
    {
        _log = logger;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<IActionResult> AuthenticateUserAsync(AuthenticationRequestDto authenticationRequest, string transactionId)
    {
        try
        {
            // Retrieve the user by username
            var userResponse = await _userRepository.GetUserByEmailAsync(authenticationRequest.Email, transactionId);

            if (userResponse.ResponseCode == 429)
            {
                _log.LogWarning(
                    "Autheticate User: Too many requests. Please try again later.",
                    transactionId
                );
                return new ObjectResult(
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message = "Too many requests. Please try again later.",
                        ErrorCode = "EC-AUTH-429",
                    }
                )
                {
                    StatusCode = 429,
                };
            }

            if (userResponse.ResponseCode == 404 || userResponse.Entity == null)
            {
                _log.LogWarning($"Authentication failed: Email '{authenticationRequest.Email}' not found.", transactionId);
                return new UnauthorizedObjectResult(new
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    Metadata = new { TransactionId = transactionId },
                    ErrorCode = "EC-AUTH-401"
                });
            }

            if (userResponse.ResponseCode == 200)
            {
                var user = userResponse.Entity;

                var passwordHash = _passwordHasher.HashPassword(authenticationRequest.Password);

                if(passwordHash == user.PasswordHash)
                {
                    _log.LogInformation($"passwords match {passwordHash} ::: {user.PasswordHash}", transactionId);
                }
                // Verify the password
                if (!_passwordHasher.VerifyPassword(user.PasswordHash, authenticationRequest.Password))
                {
                    _log.LogWarning($"Authentication failed: Incorrect password for '{authenticationRequest.Email}'.", transactionId);
                    return new UnauthorizedObjectResult(new
                    {
                        Success = false,
                        Message = "Invalid username or password.",
                        Metadata = new { TransactionId = transactionId },
                        ErrorCode = "EC-AUTH-401"
                    });
                }

                // Generate JWT token
                var token = _jwtTokenService.GenerateToken(user.UserId, user.Email, new List<string>() { "user" });

                _log.LogInformation($"User '{user.Email}' authenticated successfully.", transactionId);

                return new OkObjectResult(
                    new ApiSuccessResponse<AuthenticationResponseDto>
                    {
                        Success = true,
                        Message = "User authenticated successfully.",
                        Metadata = new { TransactionId = transactionId },
                        Data = new AuthenticationResponseDto() { Token = token }
                    }
                );
            }

            throw new InvalidOperationException(
                $"Error: {userResponse.Exception.Message}",
                userResponse.Exception
            );
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Authentication error: {ex.Message}", transactionId);
            return new ObjectResult(new
            {
                Success = false,
                Message = "An unexpected error occurred during authentication.",
                Metadata = new { TransactionId = transactionId },
                ErrorCode = "EC-AUTH-500"
            })
            {
                StatusCode = 500
            };
        }
    }

    public IActionResult? ValidationUserEmailAuth(
        string email,
        ClaimsPrincipal user,
        string transactionId,
        string resourceIdentifier
    )
    {
        var userEmailClaim = user.FindFirst(ClaimTypes.Email)?.Value;

        if (userEmailClaim == null || userEmailClaim != email)
        {
            _log.LogWarning($"Resource Forbidden for email account {email}.", transactionId);
            return new ObjectResult(
                ApiFailureResponse.FailureResponse(
                    $"Resource Forbidden for email account {email}.",
                    $"${resourceIdentifier}_FORBIDDEN"
                )
            )
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }

        return null;
    }

    public IActionResult? ValidationUserIdAuth(
        string id,
        ClaimsPrincipal user,
        string transactionId,
        string resourceIdentifier
    )
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || userIdClaim != id)
        {
            return new ObjectResult(
                ApiFailureResponse.FailureResponse(
                    $"Resource Forbidden for account id {id}.",
                    $"${resourceIdentifier}_FORBIDDEN"
                )
            )
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }

        return null;
    }
}
