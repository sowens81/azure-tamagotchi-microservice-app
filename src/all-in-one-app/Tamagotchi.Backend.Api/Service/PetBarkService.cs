using Tamagotchi.Backend.Api.Models;

namespace Tamagotchi.Backend.Api.Service;

public class PetBarkService : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly Random _random = new Random();
    private readonly TimeSpan _barkIntervalMin = TimeSpan.FromMinutes(1); // Random barking between 1-5 minutes

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize the timer
        _timer = new Timer(TriggerBark, null, TimeSpan.Zero, _barkIntervalMin);
        return Task.CompletedTask;
    }

    // Make 'state' parameter nullable to match the TimerCallback delegate
    private void TriggerBark(object? state)
    {
        if (Pet.IsAlive)
        {
            // Random barking logic, simulate a bark every random time interval
            if (_random.NextDouble() > 0.5) // 50% chance to bark
            {
                Pet.Bark();
                Console.WriteLine($"The pet barks at {DateTime.Now:yyyy-MM-dd HH:mm:ss}!");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Safely stop the timer
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Safely dispose of the timer
        _timer?.Dispose();
    }
}
