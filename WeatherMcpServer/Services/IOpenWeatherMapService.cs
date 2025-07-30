using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WeatherMcpServer.Services
{
    public interface IOpenWeatherMapService
    {
        Task<JsonNode?> GetCurrentWeatherAsync(string city, string? countryCode = null);
        Task<JsonNode?> GetWeatherForecastAsync(string city, string? countryCode = null);
        Task<JsonNode?> GetWeatherAlertsAsync(string city, string? countryCode = null);
    }
}
