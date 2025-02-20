using System.Text.Json.Serialization;

namespace Tamagotchi.Backend.SharedLibrary.Models;

public class ValidationError
{
    /// <summary>
    /// The field that caused the validation error.
    /// </summary>
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    /// <summary>
    /// The error message for the validation failure.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
