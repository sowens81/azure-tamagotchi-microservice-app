using System.ComponentModel.DataAnnotations;

namespace Tamagotchi.Backend.SharedLibrary.Dto;

public class UserRegistrationDto
{
    [Required]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 100 characters."
    )]
    public string Username { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; }

    [Required]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Password must be at least 6 characters long."
    )]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Full name must be between 3 and 100 characters."
    )]
    public string FullName { get; set; }

    public DateTime DateOfBirth { get; set; }

    [Required]
    [Phone(ErrorMessage = "Please provide a valid phone number.")]
    public string PhoneNumber { get; set; }
}
