using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tamagotchi.Backend.Users.Api.Dtos;

namespace Tamagotchi.Backend.Users.Api.Services;

public interface IAuthService
{
    Task<IActionResult> AuthenticateUserAsync(AuthenticationRequestDto authenticationRequest, string transactionId);

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
