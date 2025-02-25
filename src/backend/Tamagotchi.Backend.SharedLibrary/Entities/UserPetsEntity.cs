using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamagotchi.Backend.SharedLibrary.Entities
{
    class UserPetsEntity : UserBaseEntity
    {
        public List<PetEntity> Pets { get; set; } = new List<PetEntity>();

    }
}
