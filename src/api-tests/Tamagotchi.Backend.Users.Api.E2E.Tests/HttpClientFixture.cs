using System;
using System.Net.Http;

namespace Tamagotchi.Backend.Users.Api.E2E.Tests;

public class HttpClientFixture : IDisposable
{
    public HttpClient Client { get; }

    public HttpClientFixture()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        string baseUrl =
            Environment.GetEnvironmentVariable("API_BASE_URL")
            ?? "https://localhost:7003/";
        Client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public void Dispose()
    {
        Client.Dispose(); // Ensure cleanup after tests
    }
}
