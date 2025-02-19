namespace Tamagotchi.Backend.SharedLibrary.Factories;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string userEmail, List<string> roles);
}
