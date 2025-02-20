using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tamagotchi.Backend.SharedLibrary.Models;

public class PetStatusUpdateMessage
{
    public string PetId { get; set; }
    public string Action { get; set; }
    public DateTime ActionTimestamp { get; set; }
}
