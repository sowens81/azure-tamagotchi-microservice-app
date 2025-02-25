using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tamagotchi.Backend.SharedLibrary.Factories;
using Tamagotchi.Backend.Pets.Worker.Services;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        // Register Service Bus Factory
        services.AddSingleton<IServiceBusFactory, ServiceBusFactory>();

        // Register Service Bus Receiver Service
        services.AddSingleton<IServiceBusReceiverService, ServiceBusReceiverService>();
    })
    .Build();

// Resolve service
using var scope = host.Services.CreateScope();
var receiverService = scope.ServiceProvider.GetRequiredService<IServiceBusReceiverService>();

// Start receiving messages
await receiverService.StartReceivingMessagesAsync("your-queue-name");

// Keep the application running
Console.WriteLine("Press any key to exit...");
Console.ReadKey();