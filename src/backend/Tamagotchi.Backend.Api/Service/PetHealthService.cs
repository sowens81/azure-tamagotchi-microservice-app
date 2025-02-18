using Tamagotchi.Backend.Api.Models;

namespace Tamagotchi.Backend.Api.Service;

public class PetHealthService : IHostedService, IDisposable
{
    private Timer? _timer; // Make _timer nullable
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize the _timer in the StartAsync method
        _timer = new Timer(PerformHealthCheck, null, TimeSpan.Zero, _checkInterval);
        return Task.CompletedTask;
    }

    // Update method to handle nullable 'state' parameter
    private void PerformHealthCheck(object? state)
    {
        try
        {
            Pet.UpdateHealth(); // Check and update pet health
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Pet has died.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0); // Safely stop the timer if it's not null
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose(); // Safely dispose the timer if it's not null
    }
}
