using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class WeatherApiIntegrationTests
{
    [Fact]
    public async Task GetCurrentWeather_ReturnsData()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return; // skip if no API key
        }

        var factory = new DefaultHttpClientFactory();
        var tools = new WeatherTools(factory, NullLogger<WeatherTools>.Instance);
        var result = await tools.GetCurrentWeather("London", "GB");
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsData()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return; // skip if no API key
        }

        var factory = new DefaultHttpClientFactory();
        var tools = new WeatherTools(factory, NullLogger<WeatherTools>.Instance);
        var result = await tools.GetWeatherForecast("London", "GB");
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    private class DefaultHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client = new();
        public HttpClient CreateClient(string? name = null) => _client;
    }
}
