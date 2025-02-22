namespace Tamagotchi.Backend.Users.Api.Dtos;

public class UserUpdateResponseDto
{

    public required string Email { get; set; } // Email of the user

    public required string FirstName { get; set; } // First name of the user

    public required string LastName { get; set; } // Last name of the user

    public DateTime UpdatedAt { get; set; } // Last updated date for the user's profile
}
