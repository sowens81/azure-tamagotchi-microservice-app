using System.Text.Json.Serialization;

namespace Tamagotchi.Backend.Users.Api.Dtos;

public class TokenResponse
{
    public string Token { get; set; } = String.Empty;
}
