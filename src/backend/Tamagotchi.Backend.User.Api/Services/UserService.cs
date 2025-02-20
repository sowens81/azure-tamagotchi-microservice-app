using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.SharedLibrary.Utilities;
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
            var existingUserByUsernameResponse = await _userRepository.GetUserByUsernameAsync(
                userRegistrationRequest.Username,
                transactionId
            );

            if (existingUserByUsernameResponse.ResponseCode == 200)
            {
                _log.LogWarning(
                    $"User Registration Request: User account for username {userRegistrationRequest.Username} already exists.",
                    transactionId
                );
                return new ConflictObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Conflict: Username already registered.",
                        ErrorCode = "EC-NUR-409",
                    }
                );
            }

            // Validate if the email already exists
            var existingUserByEmailResponse = await _userRepository.GetUserByEmailAsync( // Fixed the method name
                userRegistrationRequest.Email,
                transactionId
            );

            if (existingUserByEmailResponse.ResponseCode == 200)
            {
                _log.LogWarning(
                    $"User Registration Request: User account for email {userRegistrationRequest.Email} already exists",
                    transactionId
                );
                return new ConflictObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Conflict: Email address already registered.",
                        ErrorCode = "EC-NUR-409",
                    }
                );
            }

            // Create new user entity
            var userEntity = new UserEntity
            {
                UserId = IdGenerator.GenerateShortId(),
                Username = userRegistrationRequest.Username,
                Email = userRegistrationRequest.Email,
                PasswordHash = _passwordHasher.HashPassword(userRegistrationRequest.Password),
                FirstName = userRegistrationRequest.FirstName,
                LastName = userRegistrationRequest.LastName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            // Save the user to the database
            var addUserResponse = await _userRepository.AddAsync(userEntity, transactionId);

            if (addUserResponse.ResponseCode == 429)
            {
                _log.LogWarning(
                    $"User Registration Request: Too many requests, Try again later.",
                    transactionId
                );

                return new ObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Too many requests, try again later.",
                        ErrorCode = "EC-NUR-429",
                    }
                )
                {
                    StatusCode = 429,
                };
            }

            if (addUserResponse.ResponseCode == 201)
            {
                var userRegistrationResponse = new UserRegistrationResponseDto()
                {
                    UserId = userEntity.UserId,
                    Username = userEntity.Username,
                    CreatedAt = userEntity.CreatedAt,
                };

                return new CreatedResult(
                    $"/users/{userEntity.UserId}", // Specify resource location
                    new ApiSuccessResponse()
                    {
                        Success = true,
                        Message = "Successfully registered new user.",
                        Metadata = userRegistrationResponse,
                    }
                );
            }

            throw new InvalidOperationException(
                addUserResponse.Exception.Message,
                addUserResponse.Exception
            );
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"User Registration Request: An error occurred - {ex.Message}.",
                transactionId
            );
            return new ObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later.",
                    ErrorCode = "EC-NUR-500",
                }
            )
            {
                StatusCode = 500,
            };
        }
    }

    public async Task<IActionResult> DeleteUserByIdAsync(string userId, string transactionId)
    {
        try
        {
            var accountExists = await _userRepository.GetOneByIdAsync(
                userId,
                userId,
                transactionId
            );

            if (accountExists.ResponseCode != 200)
            {
                _log.LogWarning(
                    $"User Deletion Request: User account {userId} does not exist.",
                    transactionId
                );
                return new NotFoundObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Bad Request, User account does not exist.",
                        ErrorCode = "EC-DUR-404",
                    }
                );
            }

            var deleteUser = await _userRepository.DeleteAsync(userId, userId, transactionId);

            if (deleteUser.ResponseCode == 429)
            {
                _log.LogWarning(
                    $"User Deletion Request: Too many requests. Try again later.",
                    transactionId
                );

                return new ObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Too many requests, try again later.",
                        ErrorCode = "EC-DUR-429",
                    }
                )
                {
                    StatusCode = 429,
                };
            }

            if (deleteUser.ResponseCode == 204)
            {
                return new ObjectResult(
                    new ApiSuccessResponse()
                    {
                        Success = true,
                        Message = $"User ID {userId} deleted successfully",
                    }
                )
                {
                    StatusCode = 204,
                };
            }

            throw new InvalidOperationException(
                $"Error: {deleteUser.Exception.Message}",
                deleteUser.Exception
            );
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"User Deletion Request: An error occurred - {ex.Message}.",
                transactionId
            );
            return new ObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = $"User Registration Request: An error occurred.",
                    ErrorCode = "EC-DUR-500",
                }
            )
            {
                StatusCode = 500,
            };
        }
    }

    public async Task<IActionResult> GetAllUsersAsync(string transactionId)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(transactionId);

            if (users.ResponseCode == 429)
            {
                _log.LogWarning(
                    $"Get All Users Request: Too many requests. Try again later.",
                    transactionId
                );

                return new ObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Too many requests, try again later.",
                        ErrorCode = "EC-GAU-429",
                    }
                )
                {
                    StatusCode = 429,
                };
            }

            if (users.ResponseCode == 200)
            {
                return new OkObjectResult(
                    new ApiSuccessResponse()
                    {
                        Success = true,
                        Message = $"Success",
                        Metadata = users.Entity,
                    }
                );
            }

            throw new InvalidOperationException(
                $"Error: {users.Exception.Message}",
                users.Exception
            );
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                $"Get All Users Request: An error occurred - {ex.Message}.",
                transactionId
            );
            return new ObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = $"Get All Users Request: An error occurred.",
                    ErrorCode = "EC-GAU-500",
                }
            )
            {
                StatusCode = 500,
            };
        }
    }

    public async Task<IActionResult> GetUserByEmailAsync(string email, string transactionId)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(email, transactionId);
            if (user.ResponseCode == 429)
            {
                _log.LogWarning("Too many requests when fetching user by email.", transactionId);
                return new ObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Get User By Email: Too many requests, try again later.",
                        ErrorCode = "EC-GUE-429",
                    }
                ) { StatusCode = 429 };
            }

            if (user.ResponseCode == 404)
            {
                _log.LogWarning($"Get User By Email: Email Address {email} does not exist.", transactionId);

                return new NotFoundObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "User not found.",
                        ErrorCode = "EC-GUE-404",
                    }
                );
            }
            
            if (user.ResponseCode == 200 && user.Entity != null)
            {
                return new OkObjectResult(
                    new ApiSuccessResponse()
                    {
                        Success = true,
                        Message = "User retrieved successfully.",
                        Metadata = user.Entity,
                    }
                );
            }
            
            throw new InvalidOperationException($"Error: {user.Exception.Message}", user.Exception);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error fetching user by email", transactionId);
            return new ObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    ErrorCode = "EC-GUE-500",
                }
            ) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetUserByIdAsync(string userId, string transactionId)
    {
        try
        {
            var user = await _userRepository.GetOneByIdAsync(userId, userId, transactionId);
            if (user.ResponseCode != 200)
            {
                if (user.ResponseCode == 404)
                {
                    _log.LogWarning($"Get User By Id: User Id {userId} not found.", transactionId);

                    return new ObjectResult(
                        new ApiFailureResponse()
                        {
                            Success = false,
                            Message = "Too many requests, try again later.",
                            ErrorCode = "EC-GUID-404",
                        }
                    )
                    {
                        StatusCode = 404,
                    };
                }

                if (user.ResponseCode == 409)
                {
                    _log.LogWarning(
                        $"Get User By Id: Too many requests. Try again later.",
                        transactionId
                    );

                    return new ObjectResult(
                        new ApiFailureResponse()
                        {
                            Success = false,
                            Message = "Too many requests, try again later.",
                            ErrorCode = "EC-GUID-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                if (user.ResponseCode == 200)
                {
                    return new OkObjectResult(
                        new ApiSuccessResponse() { Message = "Success", Metadata = user.Entity }
                    );
                }
            }
            
            throw new InvalidOperationException($"Error: {user.Exception.Message}", user.Exception);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Get User By Id: An error occurred - {ex.Message}.", transactionId);
            return new ObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = $"User Registration Request: An error occurred.",
                    ErrorCode = "EC-GUID-500",
                }
            )
            {
                StatusCode = 500,
            };
        }
    }

    public async Task<IActionResult> GetUserByUsernameAsync(string username, string transactionId)
    {
        try
        {
            var userResponse = await _userRepository.GetUserByUsernameAsync(username, transactionId);
            if (userResponse.ResponseCode == 429)
            {
                _log.LogWarning("Too many requests when fetching user by username.", transactionId);
                return new ObjectResult(
                    new ApiFailureResponse()
                    {
                        Success = false,
                        Message = "Too many requests, try again later.",
                        ErrorCode = "EC-GUU-429",
                    }
                ) { StatusCode = 429 };
            }
            
            if (userResponse.ResponseCode == 200 && userResponse.Entity != null)
            {
                return new OkObjectResult(
                    new ApiSuccessResponse()
                    {
                        Success = true,
                        Message = "User retrieved successfully.",
                        Metadata = userResponse.Entity,
                    }
                );
            }
            
            return new NotFoundObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = "User not found.",
                    ErrorCode = "EC-GUU-404",
                }
            );
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error fetching user by username", transactionId);
            return new ObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    ErrorCode = "EC-GUU-500",
                }
            ) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> UpdateUserAsync(
        string userId,
        UserUpdateRequestDto userUpdateRequest,
        string transactionId
    )
    {
        try
        {
            var userResponse = await _userRepository.GetOneByIdAsync(userId, userId, transactionId);
            if (userResponse.ResponseCode != 200 || userResponse.Entity == null)
            {
                if (userResponse.ResponseCode == 404)
                {
                    _log.LogWarning($"Update User: User Id {userId} not found.", transactionId);

                    return new ObjectResult(
                        new ApiFailureResponse()
                        {
                            Success = false,
                            Message = $"User Id {userId} does not exist.",
                            ErrorCode = "EC-UPDU-404",
                        }
                    )
                    {
                        StatusCode = 404,
                    };
                }

                if (userResponse.ResponseCode == 429)
                {
                    _log.LogWarning(
                        $"Update User: Too many requests. Try again later.",
                        transactionId
                    );

                    return new ObjectResult(
                        new ApiFailureResponse()
                        {
                            Success = false,
                            Message = "Too many requests, try again later.",
                            ErrorCode = "EC-UPD-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                if (userResponse.ResponseCode == 200 && userResponse.Entity != null)
                {
                    var user = userResponse.Entity;
                    user.Email = userUpdateRequest.Email;
                    user.FirstName = userUpdateRequest.FirstName;
                    user.LastName = userUpdateRequest.LastName;
                    user.UpdatedAt = DateTime.Now;

                    var updatedUser = await _userRepository.UpdateAsync(
                        userId,
                        userId,
                        user,
                        transactionId
                    );

                    if (updatedUser.ResponseCode == 429)
                    {
                        _log.LogWarning(
                            $"Update User: Too many requests. Try again later.",
                            transactionId
                        );

                        return new ObjectResult(
                            new ApiFailureResponse()
                            {
                                Success = false,
                                Message = "Too many requests, try again later.",
                                ErrorCode = "EC-UPD-429",
                            }
                        )
                        {
                            StatusCode = 429,
                        };
                    }

                    if (updatedUser.ResponseCode == 200 && updatedUser.Entity != null)
                    {
                        var userRegistrationResponse = new UserUpdateResponseDto()
                        {
                            Email = updatedUser.Entity.Email,
                            FirstName = updatedUser.Entity.FirstName,
                            LastName = updatedUser.Entity.LastName,
                            UpdatedAt = updatedUser.Entity.UpdatedAt,
                        };

                        return new OkObjectResult(
                            new ApiSuccessResponse()
                            {
                                Message = "User updated successfully.",
                                Metadata = updatedUser.Entity,
                            }
                        );
                    }

                    throw new InvalidOperationException(
                        $"Error: {updatedUser.Exception.Message}",
                        updatedUser.Exception
                    );
                }
            }

            throw new InvalidOperationException(
                $"Error: {userResponse.Exception.Message}",
                userResponse.Exception
            );
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Update User: An error occurred - {ex.Message}.", transactionId);
            return new ObjectResult(
                new ApiFailureResponse()
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later.",
                    ErrorCode = "EC-UPD-500",
                }
            )
            {
                StatusCode = 500,
            };
        }
    }
}
