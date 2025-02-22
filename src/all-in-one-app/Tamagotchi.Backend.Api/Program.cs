using Tamagotchi.Backend.Api.Models;
using Tamagotchi.Backend.Api.Service;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

builder.Services.AddControllers();
builder.Services.AddHostedService<PetHealthService>();
builder.Services.AddHostedService<PetBarkService>();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    builder
        .Configuration.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddEnvironmentVariables();

var config = builder.Configuration;

var petLoaderSection = config.GetSection("PetLoader");

var name = petLoaderSection["PET_NAME"] ?? Environment.GetEnvironmentVariable("PET_NAME");
var animal = petLoaderSection["PET_ANIMAL"] ?? Environment.GetEnvironmentVariable("PET_ANIMAL");
var color = petLoaderSection["PET_COLOR"] ?? Environment.GetEnvironmentVariable("PET_COLOR");
var health =
    petLoaderSection["PET_DEFAULTHEALTH"]
    ?? Environment.GetEnvironmentVariable("PET_DEFAULTHEALTH");

if (string.IsNullOrEmpty(name))
{
    throw new InvalidOperationException("Pet Name must not be null or empty.");
}

if (string.IsNullOrEmpty(animal))
{
    throw new InvalidOperationException("Pet Animal must not be null or empty.");
}

if (string.IsNullOrEmpty(color))
{
    throw new InvalidOperationException("Pet Color must not be null or empty.");
}

if (!int.TryParse(health, out var parsedHealth) || parsedHealth < 0 || parsedHealth > 5)
{
    throw new InvalidOperationException("Pet Health must be a valid integer between 0 and 5.");
}

Pet.Initialize(name, animal, color, parsedHealth);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
