using System.ComponentModel.DataAnnotations;

namespace Tamagotchi.Backend.SharedLibrary.Dto;

public class UserResponseDto
{
    public string Message { get; set; } 
    public string UserId { get; set; } // Unique identifier for the user

    public string Username { get; set; } // Username of the user

    public string Email { get; set; } // Email of the user

    public string FirstName { get; set; } // First name of the user

    public string LastName { get; set; } // Last name of the user

    public DateTime CreatedAt { get; set; } // Date when the user was created

    public DateTime UpdatedAt { get; set; } // Last updated date for the user's profile
}
