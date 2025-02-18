using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Tamagotchi.Backend.SharedLibrary.Options;

namespace Tamagotchi.Backend.SharedLibrary.Logging;

/// <summary>
/// A logger implementation that enriches log entries with additional context,
/// such as service name, environment name, and OpenTelemetry trace information.
/// </summary>
/// <typeparam name="T">The type of the class that is being logged for.</typeparam>
public class SuperLogger<T> : ISuperLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly string _serviceName;
    private readonly string _environmentName;
    private readonly IOptions<SuperLoggerOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuperLogger{T}"/> class.
    /// </summary>
    /// <param name="options">The options containing configuration for the logger.</param>
    /// <param name="logger">The underlying logger implementation.</param>
    /// <exception cref="ArgumentNullException">Thrown when options or logger is null, 
    /// or when required options properties are not provided.</exception>
    public SuperLogger(IOptions<SuperLoggerOptions> options, ILogger<T> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        var opts = options.Value ?? throw new ArgumentNullException(nameof(options.Value));

        _serviceName = opts.ServiceName ?? throw new ArgumentNullException(nameof(opts.ServiceName));
        _environmentName = opts.EnvironmentName ?? throw new ArgumentNullException(nameof(opts.EnvironmentName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an informational message with the provided arguments.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format into the message.</param>
    public void LogInformation(string message, string transactionId, params object[] args)
    {
        LogWithEnrichment(LogLevel.Information, message, null, transactionId, args);
    }

    /// <summary>
    /// Logs a warning message with the provided arguments.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format into the message.</param>
    public void LogWarning(string message, string transactionId, params object[] args)
    {
        LogWithEnrichment(LogLevel.Warning, message, null, transactionId, args);
    }

    /// <summary>
    /// Logs an error message along with an exception and the provided arguments.
    /// </summary>
    /// <param name="exception">The exception associated with the error.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format into the message.</param>
    public void LogError(Exception? exception, string message, string transactionId, params object[] args)
    {
        LogWithEnrichment(LogLevel.Error, message, exception, transactionId, args);
    }

    /// <summary>
    /// Logs a debug message with the provided arguments.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format into the message.</param>
    public void LogDebug(string message, string transactionId, params object[] args)
    {
        LogWithEnrichment(LogLevel.Debug, message, null, transactionId, args);
    }

    /// <summary>
    /// Logs a critical message along with an exception and the provided arguments.
    /// </summary>
    /// <param name="exception">The exception associated with the critical error.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format into the message.</param>
    public void LogCritical(Exception exception, string message, string transactionId, params object[] args)
    {
        LogWithEnrichment(LogLevel.Critical, message, exception, transactionId, args);
    }

    /// <summary>
    /// Logs a message with additional enrichment, such as scope and context.
    /// </summary>
    /// <param name="logLevel">The level of the log (e.g., Information, Warning).</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the log, if any.</param>
    /// <param name="args">The arguments to format into the message.</param>
    private void LogWithEnrichment(LogLevel logLevel, string message, Exception? exception, string transactionId, params object[] args)
    {
        var activity = Activity.Current;

        var enrichedMessage = new
        {
            Timestamp = DateTime.UtcNow,
            Severity = logLevel.ToString(),
            SeverityNumber = (int)logLevel,
            ServiceName = _serviceName,
            Environment = _environmentName,
            Message = message,
            TransactionId = transactionId,
            TraceId = activity?.TraceId.ToString(),
            SpanId = activity?.SpanId.ToString(),
            TraceFlags = activity?.ActivityTraceFlags.ToString(),
            Exception = exception?.Message,
            StackTrace = exception?.StackTrace

        };

        var jsonMessage = System.Text.Json.JsonSerializer.Serialize(enrichedMessage);

        if (exception != null)
        {
            _logger.Log(logLevel, exception, jsonMessage, args);
        }
        else
        {
            _logger.Log(logLevel, jsonMessage);
        }
    }
}
