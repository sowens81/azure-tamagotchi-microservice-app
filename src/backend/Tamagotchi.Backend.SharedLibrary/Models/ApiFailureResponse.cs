using System.Text.Json.Serialization;

namespace Tamagotchi.Backend.SharedLibrary.Models;

public class ApiFailureResponse
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;

    /// <summary>
    /// A message providing additional details about the response (e.g., error or success message).
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Additional metadata related to the response.
    /// </summary>
    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    /// <summary>
    /// An error code for debugging or user feedback.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Validation errors, if applicable.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<ValidationError>? Errors { get; set; }

    /// <summary>
    /// Creates a failed ApiFailureResponse.
    /// </summary>
    public static ApiFailureResponse FailureResponse(string message, string? ErrorCode = null, object? metadata = null)
    {
        return new ApiFailureResponse
        {
            Success = false,
            Message = message,
            ErrorCode = ErrorCode,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Creates a validation failure ApiFailureResponse.
    /// </summary>
    public static ApiFailureResponse ValidationFailureResponse(string message, List<ValidationError> errors, string? ErrorCode = null, object? metadata = null)
    {
        return new ApiFailureResponse
        {
            Success = false,
            Message = message,
            ErrorCode = ErrorCode,
            Errors = errors,
            Metadata = metadata
        };
    }
}