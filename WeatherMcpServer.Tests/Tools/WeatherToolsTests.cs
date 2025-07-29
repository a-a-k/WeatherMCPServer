using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class WeatherToolsTests
{
    private class FakeHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public HttpResponseMessage Response { get; set; } = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Response);
        }
    }

    private class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;
        public TestHttpClientFactory(HttpMessageHandler handler)
        {
            _client = new HttpClient(handler);
        }
        public HttpClient CreateClient(string? name = null) => _client;
    }

    [Fact]
    public void Constructor_Throws_WhenApiKeyMissing()
    {
        Environment.SetEnvironmentVariable("OPENWEATHER_API_KEY", null);
        Assert.Throws<InvalidOperationException>(() => new WeatherTools(new TestHttpClientFactory(new FakeHandler()), NullLogger<WeatherTools>.Instance));
    }

    [Fact]
    public async Task GetCurrentWeather_UsesExpectedEndpoint()
    {
        var handler = new FakeHandler();
        var factory = new TestHttpClientFactory(handler);
        Environment.SetEnvironmentVariable("OPENWEATHER_API_KEY", "key");
        var tools = new WeatherTools(factory, NullLogger<WeatherTools>.Instance);

        await tools.GetCurrentWeather("London");

        Assert.NotNull(handler.LastRequest);
        Assert.Contains("q=London", handler.LastRequest!.RequestUri!.Query);
    }
}
