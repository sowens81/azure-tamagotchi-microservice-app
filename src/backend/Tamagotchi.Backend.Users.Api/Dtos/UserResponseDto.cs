namespace Tamagotchi.Backend.Users.Api.Dtos;

public class UserResponseDto
{
    public required string UserId { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
