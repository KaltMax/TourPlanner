using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using TourPlanner.BLL.Configurations;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Interfaces;
using TourPlanner.BLL.Exceptions;
using TourPlanner.Logging;

namespace TourPlanner.BLL.Services
{
    public class RouteService : IRouteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILoggerWrapper<RouteService> _logger;

        public RouteService(HttpClient httpClient, IOptions<OpenRouteServiceOptions> options, ILoggerWrapper<RouteService> logger)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _logger = logger;
        }

        public async Task<RouteInfo> GetRouteInfoAsync(string from, string to, string transportType)
        {
            _logger.LogDebug($"Getting route from '{from}' to '{to}' using {transportType}");

            try
            {
                var fromCoordinates = await GeocodeAsync(from);
                var toCoordinates = await GeocodeAsync(to);

                if (fromCoordinates == null || toCoordinates == null)
                {
                    _logger.LogWarning($"Failed to geocode locations. From: '{from}', To: '{to}'");
                    throw new GeocodingException("One or both locations could not be geocoded.");
                }

                var route = await GetDirectionsAsync(fromCoordinates, toCoordinates, transportType);

                _logger.LogInformation($"Route calculated: {route.Distance:F2}km, {route.EstimatedTime:F0}min");

                return route;
            }
            catch (GeocodingException)
            {
                throw;
            }
            catch (RoutingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error getting route from '{from}' to '{to}'");
                throw;
            }
        }

        private async Task<double[]?> GeocodeAsync(string location)
        {
            try
            {
                var url = $"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(location)}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Geocoding API returned status code {response.StatusCode} for '{location}'");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                var coordinates = json["features"]?[0]?["geometry"]?["coordinates"];

                if (coordinates == null || coordinates.Count() < 2)
                {
                    _logger.LogWarning($"No coordinates found in geocoding response for '{location}'");
                    return null;
                }

                return new double[]
                {
                    coordinates?[0]?.Value<double>() ?? 0,
                    coordinates?[1]?.Value<double>() ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error geocoding location '{location}'");
                return null;
            }
        }

        private async Task<RouteInfo> GetDirectionsAsync(double[] fromCoordinates, double[] toCoordinates, string transportType)
        {
            var profile = MapTransportType(transportType);

            var fromLon = fromCoordinates[0].ToString(System.Globalization.CultureInfo.InvariantCulture);
            var fromLat = fromCoordinates[1].ToString(System.Globalization.CultureInfo.InvariantCulture);
            var toLon = toCoordinates[0].ToString(System.Globalization.CultureInfo.InvariantCulture);
            var toLat = toCoordinates[1].ToString(System.Globalization.CultureInfo.InvariantCulture);

            var url = $"https://api.openrouteservice.org/v2/directions/{profile}?api_key={_apiKey}&start={fromLon},{fromLat}&end={toLon},{toLat}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Routing API returned status code {response.StatusCode}");
                    throw new RoutingException($"Routing API returned error {response.StatusCode}: {content}");
                }

                var json = JObject.Parse(content);

                var summary = json["features"]?[0]?["properties"]?["summary"];
                if (summary == null)
                {
                    _logger.LogWarning($"Invalid routing API response: No route summary");
                    throw new RoutingException("The routing API response did not contain a valid route summary.");
                }

                var directionsText = FormatDirections(json);

                return new RouteInfo
                {
                    Distance = Math.Round(summary["distance"]?.Value<double>() ?? 0) / 1000.0,
                    EstimatedTime = Math.Round(summary["duration"]?.Value<double>() ?? 0) / 60.0,
                    Directions = directionsText,
                    GeoJson = json.ToString()
                };
            }
            catch (RoutingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting directions between coordinates");
                throw new RoutingException("Failed to retrieve directions from the routing service.", ex);
            }
        }

        private string MapTransportType(string input)
        {
            return input.ToLower() switch
            {
                "car" => "driving-car",
                "bike" => "cycling-regular",
                "walking" => "foot-walking",
                _ => "foot-walking"
            };
        }

        private string FormatDirections(JObject json)
        {
            try
            {
                var steps = json["features"]?[0]?["properties"]?["segments"]?[0]?["steps"];
                var directionsList = new List<string>();

                if (steps == null)
                {
                    return string.Empty;
                }

                foreach (var step in steps)
                {
                    var instruction = step["instruction"]?.Value<string>() ?? "Unknown instruction";
                    var name = step["name"]?.Type == JTokenType.String ? step["name"]?.Value<string>() : null;
                    var distance = step["distance"]?.Value<double>() ?? 0;
                    var duration = step["duration"]?.Value<double>() ?? 0;

                    var line =
                        $"→ {instruction}" +
                        (string.IsNullOrWhiteSpace(name) || name == "-" ? "" : $" (on {name})") + "\n" +
                        $"   Distance: {distance:F0} m, Duration: {duration:F0} s";

                    directionsList.Add(line);
                }

                return string.Join("\n\n", directionsList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error formatting directions");
                return string.Empty;
            }
        }
    }
}
