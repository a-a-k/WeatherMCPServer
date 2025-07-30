using System.Threading.Tasks;
using Xunit;
using WeatherMcpServer.Services;
using System.Text.Json.Nodes;

public class WeatherToolsTests
{
    private class MockWeatherService : IOpenWeatherMapService
    {
        public string? LastCity { get; private set; }
        public string? LastCountryCode { get; private set; }
        public JsonNode? WeatherData { get; set; } = JsonNode.Parse("{}");

        public Task<JsonNode?> GetCurrentWeatherAsync(string city, string? countryCode = null)
        {
            LastCity = city;
            LastCountryCode = countryCode;
            return Task.FromResult(WeatherData);
        }

        public Task<JsonNode?> GetWeatherForecastAsync(string city, string? countryCode = null)
        {
            LastCity = city;
            LastCountryCode = countryCode;
            return Task.FromResult(WeatherData);
        }

        public Task<JsonNode?> GetWeatherAlertsAsync(string city, string? countryCode = null)
        {
            LastCity = city;
            LastCountryCode = countryCode;
            return Task.FromResult(WeatherData);
        }
    }

    [Fact]
    public async Task GetCurrentWeather_CallsServiceWithCorrectParameters()
    {
        var mockService = new MockWeatherService();
        var tools = new WeatherTools(mockService);

        await tools.GetCurrentWeather("London", "GB");

        Assert.Equal("London", mockService.LastCity);
        Assert.Equal("GB", mockService.LastCountryCode);
    }

    [Fact]
    public async Task GetWeatherForecast_CallsServiceWithCorrectParameters()
    {
        var mockService = new MockWeatherService();
        var tools = new WeatherTools(mockService);

        await tools.GetWeatherForecast("Paris", "FR");

        Assert.Equal("Paris", mockService.LastCity);
        Assert.Equal("FR", mockService.LastCountryCode);
    }

    [Fact]
    public async Task GetWeatherAlerts_CallsServiceWithCorrectParameters()
    {
        var mockService = new MockWeatherService();
        var tools = new WeatherTools(mockService);

        await tools.GetWeatherAlerts("Tokyo", "JP");

        Assert.Equal("Tokyo", mockService.LastCity);
        Assert.Equal("JP", mockService.LastCountryCode);
    }
}
