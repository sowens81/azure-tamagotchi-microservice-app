using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamagotchi.Backend.SharedLibrary.Entities;

public abstract class UserBaseEntity : CosmosBaseEntity
{
    public required string Username { get; set; }
}
