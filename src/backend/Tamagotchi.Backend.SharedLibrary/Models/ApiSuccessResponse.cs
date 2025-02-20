using System.Text.Json.Serialization;

namespace Tamagotchi.Backend.SharedLibrary.Models;

public class ApiSuccessResponse
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

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
    /// Creates a successful ApiSuccessResponse with data.
    /// </summary>
    public static ApiSuccessResponse SuccessResponse(
        string? message = null,
        object? metadata = null
    )
    {
        return new ApiSuccessResponse
        {
            Success = true,
            Message = message,
            Metadata = metadata,
        };
    }
}

public class ApiSuccessResponse<T> : ApiSuccessResponse
{
    /// <summary>
    /// The main data payload of the response.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Creates a successful ApiSuccessResponse with data.
    /// </summary>
    public static ApiSuccessResponse<T> SuccessResponse(
        T data,
        string? message = null,
        object? metadata = null
    )
    {
        return new ApiSuccessResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Metadata = metadata,
        };
    }
}
