using Isopoh.Cryptography.Argon2;

namespace Tamagotchi.Backend.SharedLibrary.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a password using the Argon2 algorithm.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The hashed password as a string.</returns>
    public string HashPassword(string password)
    {
        return Argon2.Hash(password);
    }

    /// <summary>
    /// Verifies a password against a hashed password.
    /// </summary>
    /// <param name="hashedPassword">The hashed password.</param>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>True if the password is valid; otherwise, false.</returns>
    public bool VerifyPassword(string hashedPassword, string password)
    {
        return Argon2.Verify(hashedPassword, password);
    }
}
