namespace Tamagotchi.Backend.User.Api.Dtos;

public required class UserResponseDto
{
    public required string UserId { get; set; } // Unique identifier for the user

    public required string Username { get; set; } // Username of the user

    public required string Email { get; set; } // Email of the user

    public required string FirstName { get; set; } // First name of the user

    public required string LastName { get; set; } // Last name of the user

    public DateTime CreatedAt { get; set; } // Date when the user was created

    public DateTime UpdatedAt { get; set; } // Last updated date for the user's profile
}
