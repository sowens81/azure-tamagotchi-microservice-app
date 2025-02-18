using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Middleware;
using Tamagotchi.Backend.SharedLibrary.Options;

namespace Tamagotchi.Backend.SharedLibrary.Extensions;

/// <summary>
/// Extension methods for adding and configuring telemetry services and middleware in an ASP.NET Core application.
/// </summary>
public static class TelemetryServiceCollectionExtensions
{
    /// <summary>
    /// Adds telemetry-related services to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="serviceName">The name of the service to include in telemetry logs.</param>
    /// <param name="environmentName">The name of the deployment environment (e.g., Development, Staging, Production).</param>
    /// <param name="enableRequestResponseMiddleware">
    /// Indicates whether to enable request-response logging middleware. Default is <c>false</c>.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        string serviceName,
        string environmentName,
        bool enableRequestResponseMiddleware = false
    )
    {
        services.Configure<SuperLoggerOptions>(opts =>
        {
            opts.ServiceName = serviceName;
            opts.EnvironmentName = environmentName;
        });

        // Register the SuperLogger
        services.AddTransient(typeof(ISuperLogger<>), typeof(SuperLogger<>));

        return services;
    }

    /// <summary>
    /// Adds the request-response logging middleware to the application's request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <param name="enableRequestResponseMiddleware">
    /// Indicates whether to enable request-response logging middleware. Default is <c>true</c>.
    /// </param>
    /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseRequestResponseLogging(
        this IApplicationBuilder app,
        bool enableRequestResponseMiddleware = true
    )
    {
        if (enableRequestResponseMiddleware)
        {
            // This calls app.UseMiddleware<RequestResponseLoggingMiddleware>()
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
        return app;
    }
}