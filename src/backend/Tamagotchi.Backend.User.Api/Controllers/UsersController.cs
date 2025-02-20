using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.User.Api.Dtos;
using Tamagotchi.Backend.User.Api.Services;

namespace Tamagotchi.Backend.User.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(IUserService userService, IPasswordHasher passwordHasher)
    {
        _userService = userService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="userRegistrationRequest">The user registration details.</param>
    /// <returns>Returns the created user details or an error response.</returns>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid request payload</response>
    /// <response code="409">User already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Registers a new user",
        Description = "Registers a user with the provided details if the username and email are unique.",
        OperationId = "RegisterUser",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(
        typeof(ApiSuccessResponse<UserRegistrationResponseDto>),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser(
        [FromBody] UserRegistrationRequestDto userRegistrationRequest
    )
    {
        var transactionId =
            HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();

        if (!ModelState.IsValid)
        {
            var response = new ApiFailureResponse() { Message = "", Metadata = ModelState };
            return BadRequest(response);
        }

        return await _userService.CreateUserAsync(userRegistrationRequest, transactionId);
    }

    [HttpGet]
    [Route("api/users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var transactionId =
            HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();

        return await _userService.GetAllUsersAsync(transactionId);
    }

    // GET: api/users?email=someemail@example.com
    [HttpGet]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        var transactionId =
            HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();

        return await _userService.GetUserByEmailAsync(email, transactionId);
    }

    // GET: api/users?username=someusername
    [HttpGet("username")]
    public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
    {
        var transactionId =
            HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();

        return await _userService.GetUserByUsernameAsync(username, transactionId);
    }

    [HttpGet]
    [Route("api/users/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var transactionId =
            HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();

        return await _userService.GetUserByIdAsync(id, transactionId);
    }

    [HttpPut]
    [Route("api/users/{id}")]
    public async Task<IActionResult> UpdateUser(
        string id,
        [FromBody] UserUpdateRequestDto userUpdateDto
    )
    {
        var transactionId =
            HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();

        if (!ModelState.IsValid)
        {
            var response = new ApiFailureResponse() { Message = "", Metadata = ModelState };
            return BadRequest(response);
        }

        return await _userService.UpdateUserAsync(id, userUpdateDto, transactionId);
    }

    // DELETE: api/users/{userId}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var transactionId =
            HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();

        return await _userService.DeleteUserByIdAsync(id, transactionId);
    }
}
