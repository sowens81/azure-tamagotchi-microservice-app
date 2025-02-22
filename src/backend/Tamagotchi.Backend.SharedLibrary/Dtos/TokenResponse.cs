using System.Text.Json.Serialization;

namespace Tamagotchi.Backend.SharedLibrary.Dtos;

public class TokenResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = String.Empty;
}

