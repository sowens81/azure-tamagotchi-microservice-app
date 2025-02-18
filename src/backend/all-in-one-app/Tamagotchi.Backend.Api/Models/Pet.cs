using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tamagotchi.Backend.Api.Models;

public static class Pet
{
    // Static properties to track pet data
    public static string Name { get; private set; } = string.Empty;
    public static string Animal { get; private set; } = string.Empty;
    public static string Color { get; private set; } = string.Empty;
    public static int CurrentHealth { get; private set; }
    public static DateTime LastFed { get; private set; }
    public static int BarkCount { get; private set; }
    public static bool IsAlive { get; set; } = true;

    // Feed counter (tracks number of feedings)
    private static int FeedCount { get; set; }

    // Maximum health the pet can have
    private const int MaxHealth = 5;
    private const int MaxAllowedFeedings = MaxHealth + 3; // Allow feeding 3 times beyond the max health

    // Initializes the pet data
    public static void Initialize(string name, string animal, string color, int currentHealth)
    {
        Name = name;
        Animal = animal;
        Color = color;
        CurrentHealth = currentHealth;
        LastFed = DateTime.UtcNow; // Assume pet is fed at initialization
        BarkCount = 0; // Reset bark count
        FeedCount = 0; // Reset feed count
    }

    // Feed the pet (increases health by 1, max health is 5, but with overfeeding rules)
    public static void Feed()
    {
        if (CurrentHealth < MaxHealth)
        {
            CurrentHealth++; // Increase health by 1 if below max health
        }
        else
        {
            // Increment the feed count only if the pet is at max health
            FeedCount++;

            // If the pet has been overfed by 3 or more feeds, it dies
            if (FeedCount >= 3)
            {
                IsAlive = false;
                CurrentHealth = 0;
                throw new InvalidOperationException("Your pet has been overfed and has died.");
            }
        }
        LastFed = DateTime.UtcNow; // Update last fed time
    }

    // Check if the pet's health needs to decrease (if it's been more than 24 hours since feeding)
    public static void UpdateHealth()
    {
        if ((DateTime.UtcNow - LastFed).TotalHours >= 24)
        {
            // Pet loses 1 health point if not fed within 24 hours
            if (CurrentHealth > 0)
            {
                CurrentHealth--;
            }
        }

        if (CurrentHealth == 0)
        {
            // If health is 0, the pet dies
            IsAlive = false;
            throw new InvalidOperationException("Your pet has died.");
        }
    }

    // Track a random bark event (to be triggered at random times)
    public static void Bark()
    {
        BarkCount++;
    }

    // Get the current bark count (for metrics)
    public static int GetBarkCount() => BarkCount;
}
