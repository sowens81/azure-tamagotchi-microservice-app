using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Backend.SharedLibrary.Extensions;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Filters;
using Tamagotchi.Backend.SharedLibrary.Middleware;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.SharedLibrary.Settings;
using Tamagotchi.Backend.Users.Api.Repositories;
using Tamagotchi.Backend.Users.Api.Services;
using Tamagotchi.Backend.Users.Api.Utilities;

var builder = WebApplication.CreateBuilder(args);

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
    return Environment.GetEnvironmentVariable(envVar) ?? appConfigVars[appSettingKey] ?? throw new InvalidOperationException($"Configuration value for {envVar} or {appSettingKey} is not set.");
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

builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddTelemetry(
    serviceName: "Tamagotchi.Backend.Users.Api",
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

builder.Services.AddSingleton<CosmosDbFactory>(sp =>
{
    var cosmosEndpoint = GetConfigValue("KEYVAULT_SEC_COSMOS_ENDPOINT", "SEC_COSMOS_ENDPOINT");
    var cosmosDatabaseName = GetConfigValue("KEYVAULT_SEC_COSMOS_DATABASE", "SEC_COSMOS_DATABASE");
    var cosmosContainerName = GetConfigValue("KEYVAULT_SEC_COSMOS_CONTAINER", "SEC_COSMOS_CONTAINER");
    var cosmosDbFactory = new CosmosDbFactory(
        KeyVaultFactory.GetSecret(builder.Configuration, cosmosEndpoint),
        KeyVaultFactory.GetSecret(builder.Configuration, cosmosDatabaseName),
        KeyVaultFactory.GetSecret(builder.Configuration, cosmosContainerName)
    );
    return cosmosDbFactory;
});

// Register dependencies
builder.Services.AddScoped<IUserService, UserService>(); // Scoped instance per request
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Repository should also be scoped
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationValidationService, AuthenticationValidationService>();
builder.Services.AddScoped<IDtoMapper, DtoMapper>();

var app = builder.Build();

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseCors("AllowAllOrigins");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
