using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.User.Api.Repositories;
using Tamagotchi.Backend.User.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Register dependencies
builder.Services.AddScoped<IUserService, UserService>(); // Scoped instance per request
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Repository should also be scoped
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped(typeof(ISuperLogger<>), typeof(SuperLogger<>));
builder.Services.AddScoped<ICosmosDbFactory, CosmosDbFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
