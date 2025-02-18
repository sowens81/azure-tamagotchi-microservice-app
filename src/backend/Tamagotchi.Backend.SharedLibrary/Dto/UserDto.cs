using System.ComponentModel.DataAnnotations;

namespace Tamagotchi.Backend.SharedLibrary.Dto;

public class UserDto
{
    public string UserId { get; set; } // Unique identifier for the user

    public string Username { get; set; } // Username of the user

    public string FullName { get; set; } // Full name of the user

    public string Email { get; set; } // Email address of the user

    public string PhoneNumber { get; set; } // Phone number of the user

    public DateTime DateOfBirth { get; set; } // Date of birth of the user

    public DateTime CreatedAt { get; set; } // Date when the user was created

    public DateTime UpdatedAt { get; set; } // Last updated date for the user's profile
}
