using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Tamagotchi.Backend.User.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [Route("api/users/register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody] UserRegistrationDto userRegistrationDto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Return validation errors if the model is invalid
        }

        var user = new User
        {
            Username = userRegistrationDto.Username,
            Email = userRegistrationDto.Email,
            PasswordHash = _passwordHasher.HashPassword(userRegistrationDto.Password),
            FullName = userRegistrationDto.FullName,
            DateOfBirth = userRegistrationDto.DateOfBirth,
            PhoneNumber = userRegistrationDto.PhoneNumber,
        };

        // Save the user to the database, send confirmation email, etc.
        await _userService.RegisterUserAsync(user);

        return Ok(new { message = "User registered successfully." });
    }

    [HttpGet]
    [Route("api/users")]
    public async Task<IActionResult> GetAllUsers()
    {
        // Fetch all users from the database via the service layer
        var users = await _userService.GetAllUsersAsync();

        // Check if there are any users
        if (users == null || users.Count == 0)
        {
            return NotFound(new { message = "No users found" });
        }

        // Return a successful response with the list of users
        return Ok(users);
    }

    // GET: api/users?email=someemail@example.com
    [HttpGet]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { message = "Email parameter is required" });
        }

        var user = await _userService.GetUserByEmailAsync(email);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Return the userId as the response
        return Ok(new { userId = user.UserId });
    }

    // GET: api/users?username=someusername
    [HttpGet("username")]
    public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username parameter is required" });
        }

        var user = await _userService.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Return the userId as the response
        return Ok(new { userId = user.UserId });
    }

    [HttpGet]
    [Route("api/users/{userId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var userReturnDto = new UserReturnDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };

        return Ok(userReturnDto);
    }

    [HttpPut]
    [Route("api/users/{userId}")]
    public async Task<IActionResult> UpdateUser(
        string userId,
        [FromBody] UserUpdateDto userUpdateDto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Return validation errors if the model is invalid
        }

        var user = await _userService.GetUserAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Only update the fields that have been provided
        if (!string.IsNullOrEmpty(userUpdateDto.Username))
        {
            user.Username = userUpdateDto.Username;
        }

        if (!string.IsNullOrEmpty(userUpdateDto.Email))
        {
            user.Email = userUpdateDto.Email;
        }

        if (!string.IsNullOrEmpty(userUpdateDto.FullName))
        {
            user.FullName = userUpdateDto.FullName;
        }

        if (!string.IsNullOrEmpty(userUpdateDto.PhoneNumber))
        {
            user.PhoneNumber = userUpdateDto.PhoneNumber;
        }

        if (userUpdateDto.DateOfBirth.HasValue)
        {
            user.DateOfBirth = userUpdateDto.DateOfBirth.Value;
        }

        // Save the updated user back to the database
        await _userService.UpdateUserAsync(user);

        return Ok(new { message = "User updated successfully" });
    }

    HttpPatch("{userId}")]
    public async Task<IActionResult> PatchUser(string userId, [FromBody] JsonPatchDocument<User> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest("Invalid patch document.");
        }

        // Retrieve the user from the database
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Apply the patch
        patchDoc.ApplyTo(user, ModelState);

        // Check if the patch operation was valid
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Return validation errors if any
        }

        // Save the updated user to the database
        await _userService.UpdateUserAsync(user);

        return Ok(new { message = "User updated successfully" });
    }

    // DELETE: api/users/{userId}
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        // Check if the user exists
        var user = await _userService.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            // If the user doesn't exist, return 404 Not Found
            return NotFound(new { message = "User not found" });
        }

        // Call service to delete the user
        var result = await _userService.DeleteUserAsync(userId);
        
        if (result)
        {
            // If deletion was successful, return 204 No Content (no body)
            return NoContent();
        }

        // If there was an issue deleting (e.g., due to permissions), return 500 Internal Server Error
        return StatusCode(500, new { message = "An error occurred while deleting the user." });
    }

}
