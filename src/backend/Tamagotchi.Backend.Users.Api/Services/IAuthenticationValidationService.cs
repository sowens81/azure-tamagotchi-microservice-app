using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Tamagotchi.Backend.Users.Api.Services;

public interface IAuthenticationValidationService
{
    IActionResult? ValidationUserEmailAuth(
        string email,
        ClaimsPrincipal user,
        string transactionId,
        string resourceIdentifier
    );
    IActionResult? ValidationUserIdAuth(
        string id,
        ClaimsPrincipal user,
        string transactionId,
        string resourceIdentifier
    );
}
