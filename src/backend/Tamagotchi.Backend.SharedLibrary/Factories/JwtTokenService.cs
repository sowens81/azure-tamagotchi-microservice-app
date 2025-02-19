using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tamagotchi.Backend.SharedLibrary.Factories;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        if (jwtOptions == null) throw new ArgumentNullException(nameof(jwtOptions));

        _secretKey = jwtOptions.Value.Key ?? throw new ArgumentNullException(nameof(jwtOptions.Value.Key));
        _issuer = jwtOptions.Value.Issuer ?? throw new ArgumentNullException(nameof(jwtOptions.Value.Issuer));
        _audience = jwtOptions.Value.Audience ?? throw new ArgumentNullException(nameof(jwtOptions.Value.Audience));
    }

    public string GenerateToken(string userId, string userEmail, List<string> roles)
    {
        // Create claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create the key using the secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

        // Define the signing credentials
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Define the token expiration time
        var expires = DateTime.UtcNow.AddHours(1);

        // Create the JWT token
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        // Return the serialized JWT token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

