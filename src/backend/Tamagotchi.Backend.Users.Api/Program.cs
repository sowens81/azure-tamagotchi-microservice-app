using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tamagotchi.Backend.SharedLibrary.Extensions;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Filters;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Middleware;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.SharedLibrary.Settings;
using Tamagotchi.Backend.Users.Api.Repositories;
using Tamagotchi.Backend.Users.Api.Services;
using Tamagotchi.Backend.Users.Api.Swagger;
using Tamagotchi.Backend.Users.Api.Utilities;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "Tamagotchi.Backend.Users.Api";

if (builder.Environment.IsDevelopment())
{
    builder
        .Configuration.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}
else if (builder.Environment.IsProduction())
{
    builder
        .Configuration.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddEnvironmentVariables();
var config = builder.Configuration;
var appConfigVars = config.GetSection("KEYVAULT");

string GetConfigValue(string envVar, string appSettingKey)
{
    return Environment.GetEnvironmentVariable(envVar)
        ?? appConfigVars[appSettingKey]
        ?? throw new InvalidOperationException(
            $"Configuration value for {envVar} or {appSettingKey} is not set."
        );
}

var keyVaultName = GetConfigValue("KEYVAULT_NAME", "NAME");

KeyVaultFactory.AddKeyVault(builder.Configuration, keyVaultName);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomValidationFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddSwaggerGen(options =>
{
    // Include health check routes in Swagger documentation
    options.DocInclusionPredicate(
        (docName, apiDesc) =>
        {
            if (apiDesc.RelativePath.Contains("healthz"))
            {
                return true; // Include health check endpoints in the Swagger UI
            }

            return true; // Make sure all other endpoints are included as well
        }
    );

    // Optional: Customize API info, add authentication, etc.
    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo { Title = serviceName, Version = "v1" }
    );
    options.SchemaFilter<CustomSchemaFilter>();
});

builder.Services.AddOpenApi();

builder.Services.AddTelemetry(
    serviceName: serviceName,
    environmentName: builder.Environment.EnvironmentName,
    enableRequestResponseMiddleware: true
);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    );
});

builder.Services.Configure<JwtSettings>(options =>
{
    var jwtKey = GetConfigValue("KEYVAULT_SEC_JWT_KEY", "SEC_JWT_KEY");
    var jwtIssuer = GetConfigValue("KEYVAULT_SEC_JWT_ISSUER", "SEC_JWT_ISSUER");
    var jwtAudience = GetConfigValue("KEYVAULT_SEC_JWT_AUDIENCE", "SEC_JWT_AUDIENCE");
    options.Key = KeyVaultFactory.GetSecret(builder.Configuration, jwtKey);
    options.Issuer = KeyVaultFactory.GetSecret(builder.Configuration, jwtIssuer);
    options.Audience = KeyVaultFactory.GetSecret(builder.Configuration, jwtAudience);
});

builder.Services.AddJwtAuthentication();

builder
    .Services.AddHealthChecks()
    .AddCheck("API", () => HealthCheckResult.Healthy("API is running"))
    .AddCheck<CosmosDbHealthCheckExtension>("CosmosDB", tags: new[] { "ready" }); // Use AddCheck here

// Register dependencies

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ICosmosDbFactory>(sp =>
{
    var cosmosEndpoint = GetConfigValue("KEYVAULT_SEC_COSMOS_ENDPOINT", "SEC_COSMOS_ENDPOINT");
    var cosmosDatabaseName = GetConfigValue("KEYVAULT_SEC_COSMOS_DATABASE", "SEC_COSMOS_DATABASE");
    var cosmosContainerName = GetConfigValue(
        "KEYVAULT_SEC_COSMOS_CONTAINER",
        "SEC_COSMOS_CONTAINER"
    );
    var cosmosDbFactory = new CosmosDbFactory(
        KeyVaultFactory.GetSecret(builder.Configuration, cosmosEndpoint),
        KeyVaultFactory.GetSecret(builder.Configuration, cosmosDatabaseName),
        KeyVaultFactory.GetSecret(builder.Configuration, cosmosContainerName)
    );
    return cosmosDbFactory;
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IServiceBusFactory>(p =>
{
    var serviceBusNamespace = GetConfigValue("KEYVAULT_SEC_SERVICEBUS_NAMESPACE", "SEC_SERVICEBUS_NAMESPACE");
    var serviceBusFactory = new ServiceBusFactory(
        KeyVaultFactory.GetSecret(builder.Configuration, serviceBusNamespace)
    );
    return serviceBusFactory;
});
builder.Services.AddScoped<IServiceBusService>(provider =>
{
    var queueName = GetConfigValue("KEYVAULT_SEC_SERVICEBUS_USERS_QUEUE", "SEC_SERVICEBUS_USERS_QUEUE");
    var serviceBusFactory = provider.GetRequiredService<IServiceBusFactory>();
    var logger = provider.GetRequiredService<ISuperLogger<ServiceBusService>>();

    return new ServiceBusService(KeyVaultFactory.GetSecret(builder.Configuration, queueName), serviceBusFactory, logger);
});
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IDtoMapper, DtoMapper>();

var app = builder.Build();

// Middleware for logging and security
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseCors("AllowAllOrigins");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map the health check endpoints
app.MapHealthChecks("api/healthz/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks(
    "api/healthz/ready",
    new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") }
);

app.Run();
