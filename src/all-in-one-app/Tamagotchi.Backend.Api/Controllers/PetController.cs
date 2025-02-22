using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Backend.Api.Dto;
using Tamagotchi.Backend.Api.Models;

namespace Tamagotchi.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetController : ControllerBase
{
    [HttpGet]
    public IActionResult GetPet()
    {
        var pet = new PetDto
        {
            Name = Pet.Name,
            Animal = Pet.Animal,
            Color = Pet.Color,
            CurrentHealth = Pet.CurrentHealth,
        };

        return Ok(pet);
    }

    // Feed the pet (increases health or causes death if overfed)
    [HttpPost("feed")]
    public IActionResult FeedPet()
    {
        try
        {
            if (Pet.IsAlive)
            {
                Pet.Feed(); // This will either feed the pet or throw exception if overfed

                return Ok(
                    new PetStatusDto()
                    {
                        Message = "Pet has been fed.",
                        Heath = Pet.CurrentHealth,
                        IsAlive = Pet.IsAlive,
                    }
                );
            }
            else
            {
                return BadRequest(
                    new PetStatusDto()
                    {
                        Message = "Pet is dead.",
                        Heath = Pet.CurrentHealth,
                        IsAlive = Pet.IsAlive,
                    }
                );
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new PetStatusDto()
                {
                    Message = ex.Message,
                    Heath = Pet.CurrentHealth,
                    IsAlive = Pet.IsAlive,
                }
            ); // Handle overfeeding exception
        }
    }

    // Get the current health of the pet
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        if (Pet.IsAlive)
        {
            return Ok(new PetStatusDto() { Heath = Pet.CurrentHealth, IsAlive = Pet.IsAlive });
        }
        else
        {
            return Ok(new PetStatusDto() { Heath = Pet.CurrentHealth, IsAlive = Pet.IsAlive });
        }
    }

    // Get the current bark count (for metrics)
    [HttpGet("barks")]
    public IActionResult GetBarkCount()
    {
        if (Pet.IsAlive)
        {
            return Ok($"barks: {Pet.GetBarkCount()}");
        }
        else
        {
            return BadRequest(new { Message = "Pet is dead" });
        }
    }
}
