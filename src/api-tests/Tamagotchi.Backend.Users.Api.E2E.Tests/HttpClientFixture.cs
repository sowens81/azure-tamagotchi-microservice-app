using System;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace Tamagotchi.Backend.Users.Api.E2E.Tests;

public class HttpClientFixture : IDisposable
{
    public HttpClient Client { get; }

    public HttpClientFixture()
    {
        string baseUrl;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            baseUrl =
                Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5248/";
        }
        else
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
            };

            baseUrl =
                Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://localhost:7003/";
        }

        Client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public void Dispose()
    {
        Client.Dispose(); // Ensure cleanup after tests
    }
}
