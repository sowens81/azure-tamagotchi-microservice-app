using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tamagotchi.Backend.SharedLibrary.Logging;
using Tamagotchi.Backend.SharedLibrary.Models;

namespace Tamagotchi.Backend.Users.Api.Services;

public class AuthenticationValidationService : IAuthenticationValidationService
{
    private readonly ISuperLogger<AuthenticationValidationService> _log;

    public AuthenticationValidationService(ISuperLogger<AuthenticationValidationService> logger)
    {
        _log = logger;
    }

    public IActionResult? ValidationUserEmailAuth(
        string email,
        ClaimsPrincipal user,
        string transactionId,
        string resourceIdentifier
    )
    {
        var userEmailClaim = user.FindFirst(ClaimTypes.Email)?.Value;

        if (userEmailClaim == null || userEmailClaim != email)
        {
            _log.LogWarning($"Resource Forbidden for email account {email}.", transactionId);
            return new ObjectResult(
                ApiFailureResponse.FailureResponse(
                    $"Resource Forbidden for email account {email}.",
                    $"${resourceIdentifier}_FORBIDDEN"
                )
            )
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }

        return null;
    }

    public IActionResult? ValidationUserIdAuth(
        string id,
        ClaimsPrincipal user,
        string transactionId,
        string resourceIdentifier
    )
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || userIdClaim != id)
        {
            return new ObjectResult(
                ApiFailureResponse.FailureResponse(
                    $"Resource Forbidden for account id {id}.",
                    $"${resourceIdentifier}_FORBIDDEN"
                )
            )
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }

        return null;
    }
}
