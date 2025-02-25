using System.ComponentModel.DataAnnotations;

namespace Tamagotchi.Backend.Users.Api.Dtos;

public class AuthenticationRequestDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
