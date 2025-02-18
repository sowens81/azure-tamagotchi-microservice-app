namespace Tamagotchi.Backend.SharedLibrary.Dto;

public class UserRegistrationResponseDto
{
    public required string Message { get; set; }  
    public required string UserId { get; set; }   
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; }

}
