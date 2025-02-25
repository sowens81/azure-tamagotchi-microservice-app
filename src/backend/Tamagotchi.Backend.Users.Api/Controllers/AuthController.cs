using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.Users.Api.Dtos;
using Tamagotchi.Backend.Users.Api.Services;

namespace Tamagotchi.Backend.Users.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IUserService _userService, IAuthService _authService) : ControllerBase
{
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Registers a new user",
        Description = "Registers a user with the provided details if the username and email are unique.",
        OperationId = "RegisterUser",
        Tags = new[] { "User Management" }
    )]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserRegistrationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationRequestDto userRegistrationRequest)
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _userService.CreateUserAsync(userRegistrationRequest, transactionId);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<AuthenticationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticationRequestDto authenticationRequest)
    {
        var transactionId = HttpContext.Items["TransactionId"]?.ToString() ?? Guid.NewGuid().ToString();
        return await _authService.AuthenticateUserAsync(authenticationRequest, transactionId);
    }
}
