using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamagotchi.Backend.Pets.Worker.Services;

public interface IUserService
{
    Task<bool> CreatePetsUserAsync(object message, string transactionId);
    Task<bool> DeletePetsUserAsync(object message, string transactionId);
}
