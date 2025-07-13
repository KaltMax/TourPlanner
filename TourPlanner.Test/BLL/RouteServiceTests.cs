using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net;
using System.Text;
using TourPlanner.BLL.Configurations;
using TourPlanner.BLL.Exceptions;
using TourPlanner.BLL.Services;
using TourPlanner.BLL.Interfaces;
using TourPlanner.Logging;

namespace TourPlanner.Test.BLL
{
    [TestFixture]
    public class RouteServiceTests
    {
        private ILoggerWrapper<RouteService> _logger;
        private IOptions<OpenRouteServiceOptions> _options;
        private IRouteService _routeService;
        private MockHttpMessageHandler _mockHttpHandler;
        private HttpClient _httpClient;
        private const string TEST_API_KEY = "test-api-key";

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _logger = Substitute.For<ILoggerWrapper<RouteService>>();
            _options = Substitute.For<IOptions<OpenRouteServiceOptions>>();
            _options.Value.Returns(new OpenRouteServiceOptions { ApiKey = TEST_API_KEY });

            // Initialize mock HTTP handler
            _mockHttpHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHttpHandler);

            // Create service with mocked dependencies
            _routeService = new RouteService(_httpClient, _options, _logger);
        }

        [TearDown]
        public void Cleanup()
        {
            // Dispose the HttpClient and mock handler
            _httpClient.Dispose();
            _mockHttpHandler.Dispose();
        }

        [Test]
        public async Task GetRouteInfoAsync_ShouldReturnRouteInfo_WhenAPICallsSucceed()
        {
            // Arrange
            const string from = "Vienna";
            const string to = "Salzburg";
            const string transportType = "Car";

            // Mock geocoding responses
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Vienna",
                """{"features":[{"geometry":{"coordinates":[16.3725,48.2082]}}]}""",
                HttpStatusCode.OK);

            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Salzburg",
                """{"features":[{"geometry":{"coordinates":[13.055,47.8095]}}]}""",
                HttpStatusCode.OK);

            // Mock directions response
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={TEST_API_KEY}&start=16.3725,48.2082&end=13.055,47.8095",
                """{"features":[{"properties":{"summary":{"distance":300000,"duration":10800},"segments":[{"steps":[{"instruction":"Turn right","name":"Street","distance":1000,"duration":60}]}]}}]}""",
                HttpStatusCode.OK);

            // Act
            var result = await _routeService.GetRouteInfoAsync(from, to, transportType);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Distance, Is.EqualTo(300.0)); // 300km
            Assert.That(result.EstimatedTime, Is.EqualTo(180.0)); // 180 minutes
            Assert.That(result.Directions, Is.Not.Empty);
            Assert.That(result.GeoJson, Is.Not.Empty);
        }

        [Test]
        public void GetRouteInfoAsync_ShouldThrowGeocodingException_WhenFromLocationCannotBeGeocoded()
        {
            // Arrange
            string from = "InvalidLocation";
            string to = "Salzburg";
            string transportType = "Car";

            // Mock empty response for "from" location
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=InvalidLocation",
                """{"features":[]}""",
                HttpStatusCode.OK);

            // Act & Assert
            var ex = Assert.ThrowsAsync<GeocodingException>(() =>
                _routeService.GetRouteInfoAsync(from, to, transportType));

            Assert.That(ex.Message, Does.Contain("locations could not be geocoded"));
        }

        [Test]
        public void GetRouteInfoAsync_ShouldThrowGeocodingException_WhenToLocationCannotBeGeocoded()
        {
            // Arrange
            const string from = "Vienna";
            const string to = "InvalidLocation";
            const string transportType = "Car";

            // Mock successful "from" geocoding
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Vienna",
                """{"features":[{"geometry":{"coordinates":[16.3725,48.2082]}}]}""",
                HttpStatusCode.OK);

            // Mock empty response for "to" location
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=InvalidLocation",
                """{"features":[]}""",
                HttpStatusCode.OK);

            // Act & Assert
            var ex = Assert.ThrowsAsync<GeocodingException>(() =>
                _routeService.GetRouteInfoAsync(from, to, transportType));

            Assert.That(ex.Message, Does.Contain("locations could not be geocoded"));
        }

        [Test]
        public void GetRouteInfoAsync_ShouldThrowRoutingException_WhenDirectionsAPIFails()
        {
            // Arrange
            const string from = "Vienna";
            const string to = "Salzburg";
            const string transportType = "Car";

            // Mock successful geocoding responses
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Vienna",
                """{"features":[{"geometry":{"coordinates":[16.3725,48.2082]}}]}""",
                HttpStatusCode.OK);

            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Salzburg",
                """{"features":[{"geometry":{"coordinates":[13.055,47.8095]}}]}""",
                HttpStatusCode.OK);

            // Mock failed directions API
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={TEST_API_KEY}&start=16.3725,48.2082&end=13.055,47.8095",
                """{"error":"No route found"}""",
                HttpStatusCode.BadRequest);

            // Act & Assert
            var ex = Assert.ThrowsAsync<RoutingException>(() =>
                _routeService.GetRouteInfoAsync(from, to, transportType));

            Assert.That(ex.Message, Does.Contain("Routing API returned error"));
        }

        [Test]
        public void GetRouteInfoAsync_ShouldThrowRoutingException_WhenDirectionsAPIMissingSummary()
        {
            // Arrange
            const string from = "Vienna";
            const string to = "Salzburg";
            const string transportType = "Car";

            // Mock successful geocoding responses
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Vienna",
                """{"features":[{"geometry":{"coordinates":[16.3725,48.2082]}}]}""",
                HttpStatusCode.OK);

            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Salzburg",
                """{"features":[{"geometry":{"coordinates":[13.055,47.8095]}}]}""",
                HttpStatusCode.OK);

            // Mock directions API with invalid/incomplete response
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={TEST_API_KEY}&start=16.3725,48.2082&end=13.055,47.8095",
                """{"features":[{"properties":{"nosummary":"missing data"}}]}""",
                HttpStatusCode.OK);

            // Act & Assert
            var ex = Assert.ThrowsAsync<RoutingException>(() =>
                _routeService.GetRouteInfoAsync(from, to, transportType));

            Assert.That(ex.Message, Does.Contain("did not contain a valid route summary"));
        }

        [Test]
        public async Task GetRouteInfoAsync_ShouldUseCorrectTransportType_ForCarInput()
        {
            // Arrange
            const string from = "Vienna";
            const string to = "Salzburg";
            const string transportType = "Car";

            // Mock geocoding responses
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Vienna",
                """{"features":[{"geometry":{"coordinates":[16.3725,48.2082]}}]}""",
                HttpStatusCode.OK);

            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Salzburg",
                """{"features":[{"geometry":{"coordinates":[13.055,47.8095]}}]}""",
                HttpStatusCode.OK);

            // Mock directions with expected transport type in URL
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={TEST_API_KEY}&start=16.3725,48.2082&end=13.055,47.8095",
                """{"features":[{"properties":{"summary":{"distance":300000,"duration":10800},"segments":[{"steps":[{"instruction":"Turn right","name":"Street","distance":1000,"duration":60}]}]}}]}""",
                HttpStatusCode.OK);

            // Act
            await _routeService.GetRouteInfoAsync(from, to, transportType);

            // Assert - no explicit assertion needed - the test will fail if the URL doesn't match what's expected
        }

        [Test]
        public async Task GetRouteInfoAsync_ShouldUseCorrectTransportType_ForBikeInput()
        {
            // Arrange
            string from = "Vienna";
            string to = "Salzburg";
            string transportType = "Bike";

            // Mock geocoding responses
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Vienna",
                """{"features":[{"geometry":{"coordinates":[16.3725,48.2082]}}]}""",
                HttpStatusCode.OK);

            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Salzburg",
                """{"features":[{"geometry":{"coordinates":[13.055,47.8095]}}]}""",
                HttpStatusCode.OK);

            // Mock directions with expected transport type in URL
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/v2/directions/cycling-regular?api_key={TEST_API_KEY}&start=16.3725,48.2082&end=13.055,47.8095",
                """{"features":[{"properties":{"summary":{"distance":300000,"duration":10800},"segments":[{"steps":[{"instruction":"Turn right","name":"Street","distance":1000,"duration":60}]}]}}]}""",
                HttpStatusCode.OK);

            // Act
            await _routeService.GetRouteInfoAsync(from, to, transportType);

            // Assert - the test will fail if the URL doesn't match what's expected
        }

        [Test]
        public void GetRouteInfoAsync_ShouldHandleGeocodingAPIErrorResponse()
        {
            // Arrange
            const string from = "Vienna";
            const string to = "Salzburg";
            const string transportType = "Car";

            // Mock geocoding API error
            _mockHttpHandler.SetResponse(
                $"https://api.openrouteservice.org/geocode/search?api_key={TEST_API_KEY}&text=Vienna",
                """{"error":"Invalid API key"}""",
                HttpStatusCode.Unauthorized);

            // Act & Assert
            var ex = Assert.ThrowsAsync<GeocodingException>(() =>
                _routeService.GetRouteInfoAsync(from, to, transportType));
        }
    }

    // Custom HttpMessageHandler to mock HTTP responses
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, (HttpStatusCode StatusCode, string Content)> _responses = new();

        public void SetResponse(string url, string content, HttpStatusCode statusCode)
        {
            _responses[url] = (statusCode, content);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_responses.TryGetValue(request.RequestUri!.ToString(), out var response))
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = response.StatusCode,
                    Content = new StringContent(response.Content, Encoding.UTF8, "application/json")
                });
            }

            // If URL not found in mock responses, log it and return NotFound
            Console.WriteLine($"Unmocked URL: {request.RequestUri}");
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("""{"error":"No mock response configured for this URL"}""", Encoding.UTF8, "application/json")
            });
        }
    }
}
