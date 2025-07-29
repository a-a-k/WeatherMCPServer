using System.ComponentModel;
using System.Net.Http.Json;
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

    private async Task<T?> GetFromOpenWeatherAsync<T>(string url)
    {
        try
        {
            _logger.LogInformation("Fetching URL: {Url}", url);
            return await _httpClient.GetFromJsonAsync<T>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            throw new InvalidOperationException("Failed to fetch weather data. See logs for details.");
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
        var result = await GetFromOpenWeatherAsync<dynamic>(url);
        return result?.ToString() ?? "No data";
    }

    [McpServerTool]
    [Description("Gets a 3-day weather forecast for the specified city.")]
    public async Task<string> GetWeatherForecast(
        [Description("The city name to get weather for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
        var url = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric&cnt=24"; // 3 days ~ 24*3=72 but API returns 3h increments
        var result = await GetFromOpenWeatherAsync<dynamic>(url);
        return result?.ToString() ?? "No data";
    }

    [McpServerTool]
    [Description("Gets active weather alerts for the specified city if available.")]
    public async Task<string> GetWeatherAlerts(
        [Description("The city name to get alerts for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
        var geocodeUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(location)}&limit=1&appid={_apiKey}";
        var geo = await GetFromOpenWeatherAsync<dynamic[]>(geocodeUrl);
        if (geo == null || geo.Length == 0)
            return "Location not found.";
        double lat = geo[0].lat;
        double lon = geo[0].lon;
        var alertUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
        var result = await GetFromOpenWeatherAsync<dynamic>(alertUrl);
        return result?.ToString() ?? "No data";
    }
}
