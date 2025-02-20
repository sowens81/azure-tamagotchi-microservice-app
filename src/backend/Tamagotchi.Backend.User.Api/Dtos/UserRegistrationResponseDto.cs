namespace Tamagotchi.Backend.User.Api.Dtos;

public class UserRegistrationResponseDto
{
    public required string UserId { get; set; }   
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; }

}
