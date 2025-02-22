using System.ComponentModel.DataAnnotations;

namespace Tamagotchi.Backend.Users.Api.Dtos;

public class UserUpdateRequestDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public required string Email { get; set; }

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
