using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;


namespace Tamagotchi.Backend.SharedLibrary.Entities;

public class Pet
{
    public string Id { get; set; } // Pet ID (could be GUID)

    public string Name { get; set; } // Pet name

    public string AnimalType { get; set; } // One of 8 animals (e.g., Cat, Dog, etc.)
    public string Color { get; set; } // One of 10 colors (e.g., Red, Blue, etc.)

    // Pet status attributes
    public int Hunger { get; set; } // Hunger level (0-100)
    public int Happiness { get; set; } // Happiness level (0-100)
    public int Cleanliness { get; set; } // Cleanliness level (0-100)
    public int Health { get; set; } // Health level (0-100)
    public DateTime LastFed { get; set; } // Last time the pet was fed
    public DateTime LastExercised { get; set; } // Last time the pet was exercised
    public bool IsAlive { get; set; } // Whether the pet is alive or dead
}