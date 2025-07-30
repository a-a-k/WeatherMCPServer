using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using ModelContextProtocol.Server;
using WeatherMcpServer.Services;

public class WeatherTools
{
    private readonly IOpenWeatherMapService _weatherService;

    public WeatherTools(IOpenWeatherMapService weatherService)
    {
        _weatherService = weatherService;
    }

    [McpServerTool]
    [Description("Gets current weather conditions for the specified city.")]
    public async Task<string> GetCurrentWeather(
        [Description("The city name to get weather for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        var result = await _weatherService.GetCurrentWeatherAsync(city, countryCode);
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
        var result = await _weatherService.GetWeatherForecastAsync(city, countryCode);

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
        var result = await _weatherService.GetWeatherAlertsAsync(city, countryCode);
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
            var popNode = alert?["pop"];
            if (popNode is not null)
            {
                var pop = (double)popNode;
                if (pop > 0.5)
                {
                    var date = alert?["dt_txt"]?.ToString() ?? "N/A";
                    var weather = alert?["weather"]?[0]?["description"]?.ToString() ?? "N/A";
                    alertBuilder.AppendLine($"- {date}: High probability of precipitation ({pop * 100}%) - {weather}");
                }
            }
        }

        if (alertBuilder.Length <= 35)
        {
            return $"No significant weather alerts for {city} in the next 24 hours.";
        }

        return alertBuilder.ToString();
    }
}
