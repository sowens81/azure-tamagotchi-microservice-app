using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.Users.Api.Dtos;
using Tamagotchi.Backend.Users.Api.Services;

namespace Tamagotchi.Backend.Users.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController(IUserService _userService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves all users",
        Description = "Returns a list of all registered users.",
        OperationId = "GetAllUsers",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<UserResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUsers()
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _userService.GetAllUsersAsync(transactionId);
    }

    [HttpGet("email")]
    [SwaggerOperation(
        Summary = "Retrieves a user by email",
        Description = "Returns user details for the given email.",
        OperationId = "GetUserByEmail",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _userService.GetUserByEmailAsync(email, transactionId);
    }

    [HttpGet("username")]
    [SwaggerOperation(
        Summary = "Retrieves a user by username",
        Description = "Returns user details for the given username.",
        OperationId = "GetUserByUsername",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _userService.GetUserByUsernameAsync(username, transactionId);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Retrieves a user by ID",
        Description = "Returns user details for the given ID.",
        OperationId = "GetUserById",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(string id)
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _userService.GetUserByIdAsync(id, transactionId);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Updates a user",
        Description = "Updates user details for the given ID.",
        OperationId = "UpdateUser",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserUpdateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateRequestDto userUpdateDto)
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _userService.UpdateUserAsync(id, userUpdateDto, transactionId);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Deletes a user",
        Description = "Deletes a user by ID.",
        OperationId = "DeleteUser",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    
    public async Task<IActionResult> DeleteUser(string id)
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _userService.DeleteUserByIdAsync(id, transactionId);
    }
}
