using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Tamagotchi.Backend.SharedLibrary.Entities;

namespace Tamagotchi.Backend.User.Api.Entities;

public class UserEntity : CosmosBaseEntity
{
    [Required]
    [JsonProperty(PropertyName = "username")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [JsonProperty(PropertyName = "email")]
    public required string Email { get; set; }

    [Required]
    [JsonProperty(PropertyName = "passwordHash")]
    public required string PasswordHash { get; set; }

    [Required]
    [JsonProperty(PropertyName = "firstName")]
    public required string FirstName { get; set; }

    [Required]
    [JsonProperty(PropertyName = "lastName")]
    public required string LastName { get; set; }

    [JsonProperty(PropertyName = "createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonProperty(PropertyName = "updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

}
