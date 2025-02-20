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

namespace Tamagotchi.Backend.User.Api.Services
{
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
                // Check if the username already exists
                var existingUserByUsernameResponse = await _userRepository.GetUserByUsernameAsync(
                    userRegistrationRequest.Username,
                    transactionId
                );

                if (existingUserByUsernameResponse.ResponseCode == 200)
                {
                    _log.LogWarning(
                        $"User Registration Request: Username '{userRegistrationRequest.Username}' already exists.",
                        transactionId
                    );
                    return new ConflictObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "A user with this username already exists.",
                            ErrorCode = "EC-NUR-409",
                        }
                    );
                }

                // Check if the email already exists
                var existingUserByEmailResponse = await _userRepository.GetUserByEmailAsync(
                    userRegistrationRequest.Email,
                    transactionId
                );

                if (existingUserByEmailResponse.ResponseCode == 200)
                {
                    _log.LogWarning(
                        $"User Registration Request: Email '{userRegistrationRequest.Email}' already exists.",
                        transactionId
                    );
                    return new ConflictObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "A user with this email address already exists.",
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
                        "User Registration Request: Too many requests. Please try again later.",
                        transactionId
                    );
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "Too many requests. Please try again later.",
                            ErrorCode = "EC-NUR-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                if (addUserResponse.ResponseCode == 201)
                {
                    var userRegistrationResponse = new UserRegistrationResponseDto
                    {
                        UserId = userEntity.UserId,
                        Username = userEntity.Username,
                        CreatedAt = userEntity.CreatedAt,
                    };

                    return new CreatedResult(
                        $"/users/{userEntity.UserId}",
                        new ApiSuccessResponse
                        {
                            Success = true,
                            Message = "User has been successfully registered.",
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
                    new ApiFailureResponse
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
                        $"User Deletion Request: User account '{userId}' does not exist.",
                        transactionId
                    );
                    return new NotFoundObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "User account does not exist.",
                            ErrorCode = "EC-DUR-404",
                        }
                    );
                }

                var deleteUser = await _userRepository.DeleteAsync(userId, userId, transactionId);

                if (deleteUser.ResponseCode == 429)
                {
                    _log.LogWarning(
                        "User Deletion Request: Too many requests. Please try again later.",
                        transactionId
                    );
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "Too many requests. Please try again later.",
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
                        new ApiSuccessResponse
                        {
                            Success = true,
                            Message = $"User account '{userId}' has been successfully deleted.",
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
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message = "An unexpected error occurred. Please try again later.",
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
                        "Get All Users Request: Too many requests. Please try again later.",
                        transactionId
                    );
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "Too many requests. Please try again later.",
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
                        new ApiSuccessResponse
                        {
                            Success = true,
                            Message = "All users retrieved successfully.",
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
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message =
                            "An unexpected error occurred while retrieving users. Please try again later.",
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
                    _log.LogWarning(
                        "Get User By Email: Too many requests. Please try again later.",
                        transactionId
                    );
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "Too many requests. Please try again later.",
                            ErrorCode = "EC-GUE-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                if (user.ResponseCode == 404)
                {
                    _log.LogWarning(
                        $"Get User By Email: Email '{email}' does not exist.",
                        transactionId
                    );
                    return new NotFoundObjectResult(
                        new ApiFailureResponse
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
                        new ApiSuccessResponse
                        {
                            Success = true,
                            Message = "User retrieved successfully by email.",
                            Metadata = user.Entity,
                        }
                    );
                }

                throw new InvalidOperationException(
                    $"Error: {user.Exception.Message}",
                    user.Exception
                );
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching user by email", transactionId);
                return new ObjectResult(
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message =
                            "An unexpected error occurred while fetching user by email. Please try again later.",
                        ErrorCode = "EC-GUE-500",
                    }
                )
                {
                    StatusCode = 500,
                };
            }
        }

        public async Task<IActionResult> GetUserByIdAsync(string userId, string transactionId)
        {
            try
            {
                var user = await _userRepository.GetOneByIdAsync(userId, userId, transactionId);

                // Check if too many requests
                if (user.ResponseCode == 429 || user.ResponseCode == 409)
                {
                    _log.LogWarning(
                        $"Get User By Id: Too many requests. Please try again later.",
                        transactionId
                    );
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "Too many requests. Please try again later.",
                            ErrorCode = "EC-GUID-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                // Check if user not found
                if (user.ResponseCode == 404)
                {
                    _log.LogWarning(
                        $"Get User By Id: User Id '{userId}' not found.",
                        transactionId
                    );
                    return new NotFoundObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "User not found.",
                            ErrorCode = "EC-GUID-404",
                        }
                    );
                }

                // Check if user was found successfully
                if (user.ResponseCode == 200 && user.Entity != null)
                {
                    return new OkObjectResult(
                        new ApiSuccessResponse
                        {
                            Success = true,
                            Message = "User retrieved successfully by ID.",
                            Metadata = user.Entity,
                        }
                    );
                }

                // If none of the above conditions match, throw an exception
                throw new InvalidOperationException(
                    $"Error: {user.Exception?.Message}",
                    user.Exception
                );
            }
            catch (Exception ex)
            {
                _log.LogError(
                    ex,
                    $"Get User By Id: An error occurred - {ex.Message}.",
                    transactionId
                );
                return new ObjectResult(
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message = "An unexpected error occurred. Please try again later.",
                        ErrorCode = "EC-GUID-500",
                    }
                )
                {
                    StatusCode = 500,
                };
            }
        }

        public async Task<IActionResult> GetUserByUsernameAsync(
            string username,
            string transactionId
        )
        {
            try
            {
                var userResponse = await _userRepository.GetUserByUsernameAsync(
                    username,
                    transactionId
                );

                if (userResponse.ResponseCode == 429)
                {
                    _log.LogWarning(
                        "Get User By Username: Too many requests. Please try again later.",
                        transactionId
                    );
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "Too many requests. Please try again later.",
                            ErrorCode = "EC-GUU-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                if (userResponse.ResponseCode == 200 && userResponse.Entity != null)
                {
                    return new OkObjectResult(
                        new ApiSuccessResponse
                        {
                            Success = true,
                            Message = "User retrieved successfully by username.",
                            Metadata = userResponse.Entity,
                        }
                    );
                }

                return new NotFoundObjectResult(
                    new ApiFailureResponse
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
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message = "An unexpected error occurred. Please try again later.",
                        ErrorCode = "EC-GUU-500",
                    }
                )
                {
                    StatusCode = 500,
                };
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
                // First, retrieve the existing user
                var userResponse = await _userRepository.GetOneByIdAsync(
                    userId,
                    userId,
                    transactionId
                );

                // Handle 'too many requests'
                if (userResponse.ResponseCode == 429)
                {
                    _log.LogWarning(
                        "Update User: Too many requests. Please try again later.",
                        transactionId
                    );
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "Too many requests. Please try again later.",
                            ErrorCode = "EC-UPDU-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                // Handle 'not found'
                if (userResponse.ResponseCode == 404 || userResponse.Entity == null)
                {
                    _log.LogWarning($"Update User: User Id '{userId}' not found.", transactionId);
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = $"User Id '{userId}' does not exist.",
                            ErrorCode = "EC-UPDU-404",
                        }
                    )
                    {
                        StatusCode = 404,
                    };
                }

                // If user is successfully retrieved (200)
                if (userResponse.ResponseCode == 200 && userResponse.Entity != null)
                {
                    var user = userResponse.Entity;
                    user.Email = userUpdateRequest.Email;
                    user.FirstName = userUpdateRequest.FirstName;
                    user.LastName = userUpdateRequest.LastName;
                    user.UpdatedAt = DateTime.Now;

                    // Attempt to update
                    var updatedUser = await _userRepository.UpdateAsync(
                        userId,
                        userId,
                        user,
                        transactionId
                    );

                    if (updatedUser.ResponseCode == 429)
                    {
                        _log.LogWarning(
                            "Update User: Too many requests. Please try again later.",
                            transactionId
                        );
                        return new ObjectResult(
                            new ApiFailureResponse
                            {
                                Success = false,
                                Message = "Too many requests. Please try again later.",
                                ErrorCode = "EC-UPDU-429",
                            }
                        )
                        {
                            StatusCode = 429,
                        };
                    }

                    if (updatedUser.ResponseCode == 200 && updatedUser.Entity != null)
                    {
                        var userUpdateResponse = new UserUpdateResponseDto
                        {
                            Email = updatedUser.Entity.Email,
                            FirstName = updatedUser.Entity.FirstName,
                            LastName = updatedUser.Entity.LastName,
                            UpdatedAt = updatedUser.Entity.UpdatedAt,
                        };

                        return new OkObjectResult(
                            new ApiSuccessResponse
                            {
                                Success = true,
                                Message = "User updated successfully.",
                                Metadata = userUpdateResponse,
                            }
                        );
                    }

                    throw new InvalidOperationException(
                        $"Error: {updatedUser.Exception?.Message}",
                        updatedUser.Exception
                    );
                }

                // If we reach here but haven't returned, throw the repository exception
                throw new InvalidOperationException(
                    $"Error: {userResponse.Exception?.Message}",
                    userResponse.Exception
                );
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Update User: An error occurred - {ex.Message}.", transactionId);
                return new ObjectResult(
                    new ApiFailureResponse
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
}
