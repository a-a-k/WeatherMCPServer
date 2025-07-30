using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System;

namespace WeatherMcpServer.Services
{
    /// <summary>
    /// A service to interact with the OpenWeatherMap API.
    /// </summary>
    public class OpenWeatherMapService : IOpenWeatherMapService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenWeatherMapService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenWeatherMapService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger.</param>
        public OpenWeatherMapService(IHttpClientFactory httpClientFactory, ILogger<OpenWeatherMapService> logger)
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

        /// <inheritdoc />
        public async Task<JsonNode?> GetCurrentWeatherAsync(string city, string? countryCode = null)
        {
            var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric";
            return await GetFromOpenWeatherAsync(url);
        }

        /// <inheritdoc />
        public async Task<JsonNode?> GetWeatherForecastAsync(string city, string? countryCode = null)
        {
            var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric&cnt=24";
            return await GetFromOpenWeatherAsync(url);
        }

        /// <inheritdoc />
        public async Task<JsonNode?> GetWeatherAlertsAsync(string city, string? countryCode = null)
        {
            var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
            var geocodeUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(location)}&limit=1&appid={_apiKey}";
            var geoResult = await GetFromOpenWeatherAsync(geocodeUrl);
            if (geoResult is null || geoResult.AsArray().Count == 0)
            {
                return null;
            }

            var latNode = geoResult[0]?["lat"];
            var lonNode = geoResult[0]?["lon"];

            if (latNode is null || lonNode is null)
            {
                return null;
            }

            var lat = (double)latNode;
            var lon = (double)lonNode;

            var url = $"https://api.openweathermap.org/data/2.5/forecast/hourly?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&cnt=24";
            return await GetFromOpenWeatherAsync(url);
        }
    }
}
