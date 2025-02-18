using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Tamagotchi.Backend.SharedLibrary.Entities;

public class User
{
    public string Id { get; set; } // User ID (could be GUID or username)

    public string Name { get; set; }
    public string Email { get; set; }

    public List<Pet> Pets { get; set; } // List of pets the user owns
}
