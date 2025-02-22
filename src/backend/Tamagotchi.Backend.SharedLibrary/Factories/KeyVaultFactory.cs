using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Tamagotchi.Backend.SharedLibrary.Factories;

public static class KeyVaultFactory
{
    /// <summary>
    /// Configures Azure Key Vault as a configuration source.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder to add Key Vault to.</param>
    /// <param name="keyVaultName">The name of the Azure Key Vault.</param>
    public static void AddKeyVault(IConfigurationBuilder configurationBuilder, string keyVaultName)
    {
        if (string.IsNullOrEmpty(keyVaultName))
        {
            throw new InvalidOperationException("KeyVault name is not configured.");
        }

        var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";

        // Add Key Vault as a configuration source
        configurationBuilder.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
    }
    public static string GetSecret(IConfiguration configuration, string key)
    {
        var secret = configuration[key];
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException($"Secret {key} is not configured in Key Vault.");
        }
        return secret;
    }

}