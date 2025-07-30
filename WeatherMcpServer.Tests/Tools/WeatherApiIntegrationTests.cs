using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using WeatherMcpServer.Services;

public class WeatherApiIntegrationTests
{
    private class DefaultHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client = new();
        public HttpClient CreateClient(string? name = null) => _client;
    }

    [Fact]
    public void Constructor_Throws_WhenApiKeyMissing()
    {
        Environment.SetEnvironmentVariable("OPENWEATHER_API_KEY", null);
        var factory = new DefaultHttpClientFactory();
        var logger = NullLogger<OpenWeatherMapService>.Instance;
        Assert.Throws<InvalidOperationException>(() => new OpenWeatherMapService(factory, logger));
    }

    [Fact]
    public async Task GetCurrentWeather_ReturnsData()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // This test requires an API key.
            // We've already tested that an exception is thrown when the key is missing.
            // So we can skip this test if the key is not present.
            return;
        }

        var factory = new DefaultHttpClientFactory();
        var logger = NullLogger<OpenWeatherMapService>.Instance;
        var service = new OpenWeatherMapService(factory, logger);
        var tools = new WeatherTools(service);
        var result = await tools.GetCurrentWeather("London", "GB");
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsData()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return;
        }

        var factory = new DefaultHttpClientFactory();
        var logger = NullLogger<OpenWeatherMapService>.Instance;
        var service = new OpenWeatherMapService(factory, logger);
        var tools = new WeatherTools(service);
        var result = await tools.GetWeatherForecast("London", "GB");
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task GetWeatherAlerts_ReturnsData()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return;
        }

        var factory = new DefaultHttpClientFactory();
        var logger = NullLogger<OpenWeatherMapService>.Instance;
        var service = new OpenWeatherMapService(factory, logger);
        var tools = new WeatherTools(service);
        var result = await tools.GetWeatherAlerts("London", "GB");
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
