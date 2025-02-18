namespace Tamagotchi.Backend.SharedLibrary.Options;

/// <summary>
/// Configuration options for the <see cref="SuperLogger{T}"/> class.
/// </summary>
public class SuperLoggerOptions
{
    /// <summary>
    /// Gets or sets the name of the service. This is used to enrich log entries.
    /// </summary>
    /// <value>
    /// The service name, typically representing the application or subsystem being logged.
    /// Defaults to an empty string.
    /// </value>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment name. This is used to indicate the deployment environment (e.g., Development, Staging, Production).
    /// </summary>
    /// <value>
    /// The environment name. Defaults to an empty string.
    /// </value>
    public string EnvironmentName { get; set; } = string.Empty;
}
