namespace Tamagotchi.Backend.Api.Dto;

public class PetStatusDto
{
    public int Heath { get; set; }
    public bool IsAlive { get; set; }
    public string? Message { get; set; }
}
