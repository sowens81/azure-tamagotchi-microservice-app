using System.Security.Cryptography;

namespace Tamagotchi.Backend.SharedLibrary.Utilities;

public static class IdGenerator
{
    public static string GenerateShortId(int length = 20)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[]? id = null;

        if (length < 20)
        {
            length = 5;
        }
        else
        {
            length = length - 15;
        }

        var data = new byte[length];
        RandomNumberGenerator.Fill(data);

        id = new char[length];
        for (int i = 0; i < length; i++)
        {
            id[i] = chars[data[i] % chars.Length];
        }

        // Add a timestamp to the end of the ID
        string timestamp = DateTime.UtcNow.Ticks.ToString("x"); // Hexadecimal format for brevity
        return new string(id) + timestamp.ToUpper();
    }
}
