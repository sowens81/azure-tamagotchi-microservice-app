namespace Tamagotchi.Backend.SharedLibrary.Models;

/// <summary>
/// Represents trace information for telemetry purposes.
/// </summary>
public class TraceInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the trace.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the span within the trace.
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// Gets or sets the trace flags used to control tracing behaviour.
    /// </summary>
    public string? TraceFlags { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the transaction associated with the trace.
    /// </summary>
    public string? TransactionId { get; set; }
}

