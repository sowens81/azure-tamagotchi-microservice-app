using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.SharedLibrary.Utilities;
using Tamagotchi.Backend.Users.Api.Dtos;
using Tamagotchi.Backend.Users.Api.Entities;
using Tamagotchi.Backend.Users.Api.Repositories;
using Tamagotchi.Backend.Users.Api.Utilities;

namespace Tamagotchi.Backend.Users.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISuperLogger<UserService> _log;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IDtoMapper _dtoMapper;
        private readonly IServiceBusService _serviceBusService;

        public UserService(
            IUserRepository repository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            ISuperLogger<UserService> logger,
            IDtoMapper dtoMapper,
            IServiceBusService serviceBusService
        )
        {
            _userRepository = repository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _log = logger;
            _dtoMapper = dtoMapper;
            _serviceBusService = serviceBusService;
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
                            Metadata = new { TransactionId = transactionId },
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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-NUR-409",
                        }
                    );
                }

                var userEntity = _dtoMapper.MapUserRegistrationRequestToUserEntity(userRegistrationRequest);

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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-NUR-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                if (addUserResponse.ResponseCode == 201)
                {

                    var message = new SbusMessage<UserRegistration>()
                    {
                        MessageType = "USER_REGISTER",
                        TransactionId = transactionId,
                        Payload = new UserRegistration()
                        {
                            UserId = userEntity.UserId,
                            Username = userEntity.Username
                        }
                    };

                    var sendMessage = _serviceBusService.SendMessageAsync(message.MessageType, message, transactionId);

                    return new CreatedResult(
                        $"/users/{userEntity.UserId}",
                        new ApiSuccessResponse<UserRegistrationResponseDto>
                        {
                            Success = true,
                            Message = "User has been successfully registered.",
                            Metadata = new { TransactionId = transactionId },
                            Data = _dtoMapper.MapUserEntityToUserRegistrationResponse(userEntity)
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
                        Message = $"An unexpected error occurred. Please try again later: {ex.Message}",
                        Metadata = new { TransactionId = transactionId },
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
                var deleteUser = await _userRepository.DeleteAsync(userId, userId, transactionId);

                if (deleteUser.ResponseCode == 404)
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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-DUR-404",
                        }
                    );
                }

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
                            Metadata = new { TransactionId = transactionId },
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
                            Metadata = new { TransactionId = transactionId },
                        }
                    )
                    {
                        StatusCode = 202,
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
                        Message = $"An unexpected error occurred. Please try again later: {ex.Message}",
                        Metadata = new { TransactionId = transactionId },
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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-GAU-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }


                if (users.ResponseCode == 200)
                {
                    var usersResponse = new List<UserResponseDto>();

                    if (users?.Entity != null && users.Entity.Any())
                    {
                        usersResponse = _dtoMapper.MapUserEntitiesToListUserResponse(users.Entity);
                    }

                        return new OkObjectResult(
                        new ApiSuccessResponse<List<UserResponseDto>>
                        {
                            Success = true,
                            Message = "All users retrieved successfully.",
                            Metadata = new { TransactionId = transactionId },
                            Data = usersResponse,
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
                            $"An unexpected error occurred. Please try again later: {ex.Message}",
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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-GUE-404",
                        }
                    );
                }

                if (user.ResponseCode == 200 && user.Entity != null)
                {
                    return new OkObjectResult(
                        new ApiSuccessResponse<UserResponseDto>
                        {
                            Success = true,
                            Message = "User retrieved successfully by email.",
                            Metadata = new { TransactionId = transactionId },
                            Data = _dtoMapper.MapUserEntityToUserResponse(user.Entity),
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
                            $"An unexpected error occurred. Please try again later: {ex.Message}",
                        Metadata = new { TransactionId = transactionId },
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
                if (user.ResponseCode == 429)
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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-GUID-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                // Check if user not found
                if (user.ResponseCode == 404 || user.Entity == null)
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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-GUID-404",
                        }
                    );
                }

                // Check if user was found successfully
                if (user.ResponseCode == 200 && user.Entity != null)
                {

                    return new OkObjectResult(
                        new ApiSuccessResponse<UserResponseDto>
                        {
                            Success = true,
                            Message = "User retrieved successfully by ID.",
                            Metadata = new { TransactionId = transactionId },
                            Data = _dtoMapper.MapUserEntityToUserResponse(user.Entity),
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
                        Message = $"An unexpected error occurred. Please try again later: {ex.Message}",
                        Metadata = new { TransactionId = transactionId },
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
                var user = await _userRepository.GetUserByUsernameAsync(username, transactionId);

                if (user.ResponseCode == 429)
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
                            ErrorCode = "EC-GUE-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                if (user.ResponseCode == 404 || user.Entity == null)
                {
                    _log.LogWarning(
                        $"Get User By Username: Username '{username}' does not exist.",
                        transactionId
                    );
                    return new NotFoundObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = "User not found.",
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-GUE-404",
                        }
                    );
                }

                if (user.ResponseCode == 200 && user.Entity != null)
                {
                    return new OkObjectResult(
                        new ApiSuccessResponse<UserResponseDto>
                        {
                            Success = true,
                            Message = "User retrieved successfully by username.",
                            Metadata = new { TransactionId = transactionId },
                            Data = _dtoMapper.MapUserEntityToUserResponse(user.Entity),
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
                _log.LogError(ex, "Get User By Username: Error fetching user by username", transactionId);
                return new ObjectResult(
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message =
                            $"An unexpected error occurred. Please try again later: {ex.Message}",
                        Metadata = new { TransactionId = transactionId },
                        ErrorCode = "EC-GUE-500",
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
                var user = await _userRepository.GetOneByIdAsync(
                    userId,
                    userId,
                    transactionId
                );

                // Handle 'too many requests'
                if (user.ResponseCode == 429)
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
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-UPDU-429",
                        }
                    )
                    {
                        StatusCode = 429,
                    };
                }

                // Handle 'not found'
                if (user.ResponseCode == 404 || user.Entity == null)
                {
                    _log.LogWarning($"Update User: User Id '{userId}' not found.", transactionId);
                    return new ObjectResult(
                        new ApiFailureResponse
                        {
                            Success = false,
                            Message = $"User Id '{userId}' does not exist.",
                            Metadata = new { TransactionId = transactionId },
                            ErrorCode = "EC-UPDU-404",
                        }
                    )
                    {
                        StatusCode = 404,
                    };
                }

                if (user.ResponseCode == 200 && user.Entity != null)
                {
                        
                    var updatedUser = await _userRepository.UpdateAsync(
                        userId,
                        userId,
                        _dtoMapper.CombineUserUpdateRequsestWithUserEntity(userUpdateRequest, user.Entity),
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
                                Metadata = new { TransactionId = transactionId },
                                ErrorCode = "EC-UPDU-429",
                            }
                        )
                        {
                            StatusCode = 429,
                        };
                    }

                    if (updatedUser.ResponseCode == 200 && updatedUser.Entity != null)
                    {
                        return new OkObjectResult(
                            new ApiSuccessResponse<UserUpdateResponseDto>
                            {
                                Success = true,
                                Message = "User updated successfully.",
                                Metadata = new { TransactionId = transactionId },
                                Data = _dtoMapper.MapUserEntityToUserUpdateResonse(updatedUser.Entity),
                            }
                        );
                    }

                    throw new InvalidOperationException(
                        $"Error: {updatedUser.Exception?.Message}",
                        updatedUser.Exception
                    );
                }

                throw new InvalidOperationException(
                    $"Error: {user.Exception?.Message}",
                    user.Exception
                );
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Update User: An error occurred - {ex.Message}.", transactionId);
                return new ObjectResult(
                    new ApiFailureResponse
                    {
                        Success = false,
                        Message = $"An unexpected error occurred. Please try again later: {ex.Message}",
                        Metadata = new { TransactionId = transactionId },
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
