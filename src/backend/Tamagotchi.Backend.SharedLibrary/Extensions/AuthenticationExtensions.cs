using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tamagotchi.Backend.SharedLibrary.Attributes;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Settings;

namespace Tamagotchi.Backend.SharedLibrary.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Resolve JwtSettings via dependency injection
                var serviceProvider = services.BuildServiceProvider();
                var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var resourceIdentifier = context.HttpContext.GetEndpoint()?
                            .Metadata.GetMetadata<ResourceFilterIdentifier>()?.ResourceIdentifier;

                        var errorCode = string.IsNullOrEmpty(resourceIdentifier)
                            ? "UNAUTH"
                            : $"{resourceIdentifier}_UNAUTH";

                        var response = ApiFailureResponse.FailureResponse(
                            "Unauthorized access. Authentication is required.",
                            errorCode
                        );

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(response);
                    },
                    OnForbidden = async context =>
                    {
                        var resourceIdentifier = context.HttpContext.GetEndpoint()?
                            .Metadata.GetMetadata<ResourceFilterIdentifier>()?.ResourceIdentifier;

                        var errorCode = string.IsNullOrEmpty(resourceIdentifier)
                            ? "FORBIDDEN"
                            : $"{resourceIdentifier}_FORBIDDEN";

                        var response = ApiFailureResponse.FailureResponse(
                            "You do not have permission to access this resource.",
                            errorCode
                        );

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(response);
                    }
                };
            });

        return services;
    }
}