using System.ComponentModel.DataAnnotations;

namespace Tamagotchi.Backend.SharedLibrary.Dto;

public class UserUpdateRequestDto
{
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 100 characters."
    )]
    public string Username { get; set; }

    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; }

    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Full name must be between 3 and 100 characters."
    )]
    public string FullName { get; set; }

    [Phone(ErrorMessage = "Please provide a valid phone number.")]
    public string PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; } // Optional field for updating DOB

    // You can also add additional fields here as needed, for example:
    // public string Address { get; set; }
}
