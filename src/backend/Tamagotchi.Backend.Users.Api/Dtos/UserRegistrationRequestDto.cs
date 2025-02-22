using System.ComponentModel.DataAnnotations;

namespace Tamagotchi.Backend.Users.Api.Dtos;

public class UserRegistrationRequestDto
{
    [Required]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 100 characters."
    )]
    public required string Username { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public required string Email { get; set; }

    [Required]
    [StringLength(
        100,
        MinimumLength = 10,
        ErrorMessage = "Password must be at least 10 characters long."
    )]
    public required string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }

    [Required]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "First name must be between 3 and 100 characters."
    )]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Last name must be between 3 and 100 characters."
    )]
    public required string LastName { get; set; }
}
