using Newtonsoft.Json;
using Tamagotchi.Backend.SharedLibrary.Utilities;

namespace Tamagotchi.Backend.SharedLibrary.Entities;

public class CosmosBaseEntity
{
    [JsonProperty(PropertyName = "id")]
    public required string UserId { get; set; } = IdGenerator.GenerateShortId();
}
