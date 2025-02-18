using Microsoft.AspNetCore.Http;
using Tamagotchi.Backend.SharedLibrary.Logging;

namespace Tamagotchi.Backend.SharedLibrary.Middleware;

/// <summary>
/// Middleware for logging HTTP request and response details, enriched with OpenTelemetry context and TransactionId.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISuperLogger<RequestResponseLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestResponseLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the HTTP pipeline.</param>
    /// <param name="logger">The logger to log request and response details.</param>
    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ISuperLogger<RequestResponseLoggingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Middleware invocation method for processing the HTTP request and response.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or retrieve the TransactionId from the request headers or create a new one if missing
        var transactionId = context.Request.Headers["TransactionId"].ToString();
        if (string.IsNullOrEmpty(transactionId))
        {
            transactionId = Guid.NewGuid().ToString(); // Create a new TransactionId if not provided
        }

        // Attach the TransactionId to the HttpContext so that it can be accessed by other parts of the app
        context.Items["TransactionId"] = transactionId;

        // Log Request
        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0; // Reset the stream position for further reading in the pipeline

        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;
        _logger.LogInformation(
            $"Request: {requestMethod} {requestPath} {requestBody}",
            transactionId
        );

        // Capture and log Response
        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context); // Invoke the next middleware in the pipeline

        // Read and log the response body
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin); // Reset the stream position for the client

        var statusCode = context.Response.StatusCode;
        _logger.LogInformation(
            $"Response: {statusCode} {responseBody}",
            transactionId
        );

        // Copy the response body back to the original stream
        await responseBodyStream.CopyToAsync(originalResponseBodyStream);
    }
}
