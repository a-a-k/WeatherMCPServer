using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

public class WeatherTools
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<WeatherTools> _logger;

    public WeatherTools(IHttpClientFactory httpClientFactory, ILogger<WeatherTools> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? throw new InvalidOperationException("OPENWEATHER_API_KEY environment variable not set.");
        _logger = logger;
    }

    private async Task<JsonNode?> GetFromOpenWeatherAsync(string url)
    {
        try
        {
            _logger.LogInformation("Fetching URL: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JsonNode>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            return null;
        }
    }

    [McpServerTool]
    [Description("Gets current weather conditions for the specified city.")]
    public async Task<string> GetCurrentWeather(
        [Description("The city name to get weather for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric";
        var result = await GetFromOpenWeatherAsync(url);
        if (result is null)
        {
            return "Failed to fetch weather data.";
        }

        var weather = result["weather"]?[0]?["description"]?.ToString() ?? "N/A";
        var temp = result["main"]?["temp"]?.ToString() ?? "N/A";
        var humidity = result["main"]?["humidity"]?.ToString() ?? "N/A";

        return $"Current weather in {city}: {weather}, Temperature: {temp}°C, Humidity: {humidity}%";
    }

    [McpServerTool]
    [Description("Gets a 3-day weather forecast for the specified city.")]
    public async Task<string> GetWeatherForecast(
        [Description("The city name to get weather for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
        var url = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric&cnt=24"; // 3 days ~ 24*3=72 but API returns 3h increments
        var result = await GetFromOpenWeatherAsync(url);

        if (result is null)
        {
            return "Failed to fetch weather forecast.";
        }

        var forecastBuilder = new StringBuilder();
        forecastBuilder.AppendLine($"3-day weather forecast for {city}:");

        var forecasts = result["list"]?.AsArray();
        if (forecasts is not null)
        {
            foreach (var forecast in forecasts)
            {
                var date = forecast?["dt_txt"]?.ToString() ?? "N/A";
                var weather = forecast?["weather"]?[0]?["description"]?.ToString() ?? "N/A";
                var temp = forecast?["main"]?["temp"]?.ToString() ?? "N/A";
                forecastBuilder.AppendLine($"- {date}: {weather}, {temp}°C");
            }
        }

        return forecastBuilder.ToString();
    }

    [McpServerTool]
    [Description("Gets active weather alerts for the specified city if available. NOTE: This is a simulated implementation using the hourly forecast endpoint due to API key limitations.")]
    public async Task<string> GetWeatherAlerts(
        [Description("The city name to get alerts for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
        var geocodeUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(location)}&limit=1&appid={_apiKey}";
        var geoResult = await GetFromOpenWeatherAsync(geocodeUrl);
        if (geoResult is null || geoResult.AsArray().Count == 0)
        {
            return "Location not found.";
        }
        var lat = (double)geoResult[0]["lat"];
        var lon = (double)geoResult[0]["lon"];

        var url = $"https://api.openweathermap.org/data/2.5/forecast/hourly?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&cnt=24";
        var result = await GetFromOpenWeatherAsync(url);
        if (result is null)
        {
            return "Failed to fetch weather alerts.";
        }

        var alerts = result["list"]?.AsArray();
        if (alerts is null || alerts.Count == 0)
        {
            return $"No active weather alerts for {city}.";
        }

        var alertBuilder = new StringBuilder();
        alertBuilder.AppendLine($"Simulated weather alerts for {city}:");

        foreach (var alert in alerts)
        {
            var pop = (double)(alert?["pop"] ?? 0);
            if (pop > 0.5)
            {
                var date = alert?["dt_txt"]?.ToString() ?? "N/A";
                var weather = alert?["weather"]?[0]?["description"]?.ToString() ?? "N/A";
                alertBuilder.AppendLine($"- {date}: High probability of precipitation ({pop * 100}%) - {weather}");
            }
        }

        if (alertBuilder.Length <= 35)
        {
            return $"No significant weather alerts for {city} in the next 24 hours.";
        }

        return alertBuilder.ToString();
    }
}
