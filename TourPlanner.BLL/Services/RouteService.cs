using System.Text.Json;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Interfaces;
using TourPlanner.BLL.Exceptions;
using TourPlanner.Logging;

namespace TourPlanner.BLL.Services
{
    public class RouteService : IRouteService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly ILoggerWrapper<RouteService> _logger;

        public RouteService(HttpClient httpClient, ILoggerWrapper<RouteService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<RouteInfo> GetRouteInfoAsync(string from, string to, string transportType, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"Getting route from '{from}' to '{to}' using {transportType}");

            try
            {
                transportType = MapTransportType(transportType);

                var fromCoordinates = await GeocodeAsync(from, cancellationToken);
                if (fromCoordinates == null)
                {
                    _logger.LogWarning($"Location could not be geocoded: '{from}'");
                    throw new GeocodingException("One or both locations could not be geocoded.");
                }

                var toCoordinates = await GeocodeAsync(to, cancellationToken);
                if (toCoordinates == null)
                {
                    _logger.LogWarning($"Location could not be geocoded: '{to}'");
                    throw new GeocodingException("One or both locations could not be geocoded.");
                }

                var route = await GetDirectionsAsync(fromCoordinates, toCoordinates, transportType, cancellationToken);

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
            catch (UnsupportedTransportTypeException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                // The caller cancelled
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error getting route from '{from}' to '{to}'");
                throw;
            }
        }

        private async Task<double[]?> GeocodeAsync(string location, CancellationToken cancellationToken)
        {
            var url = $"geocode/search?text={Uri.EscapeDataString(location)}";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(url, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Genuine caller cancellation (not an HttpClient timeout)
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Network failure / timeout reaching the provider is an upstream problem,
                // not a bad address; surface it as a 502 via RoutingException.
                _logger.LogError(ex, $"Network error contacting the geocoding service for '{location}'");
                throw new RoutingException($"Failed to reach the geocoding service for '{location}'.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                // A non-success status (auth error, rate limit, provider outage, ...) is an
                // upstream failure, not an unresolvable address; surface it as a 502.
                _logger.LogWarning($"Geocoding API returned status code {response.StatusCode} for '{location}'");
                throw new RoutingException($"Geocoding service returned status {(int)response.StatusCode} for '{location}'.");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            OrsGeocodeResponse? geocode;
            try
            {
                geocode = JsonSerializer.Deserialize<OrsGeocodeResponse>(content, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Invalid JSON received from the geocoding service for '{location}'");
                throw new RoutingException($"Geocoding service returned an invalid response for '{location}'.", ex);
            }

            var coordinates = geocode?.Features?.FirstOrDefault()?.Geometry?.Coordinates;
            if (coordinates == null || coordinates.Length < 2)
            {
                // A successful response with no coordinates means the address itself could not
                // be resolved. Returning null lets the caller map this to a 400 Bad Request.
                _logger.LogWarning($"No coordinates found in geocoding response for '{location}'");
                return null;
            }

            return [coordinates[0], coordinates[1]];
        }

        private async Task<RouteInfo> GetDirectionsAsync(double[] fromCoordinates, double[] toCoordinates, string transportType, CancellationToken cancellationToken)
        {
            var fromLon = fromCoordinates[0].ToString(System.Globalization.CultureInfo.InvariantCulture);
            var fromLat = fromCoordinates[1].ToString(System.Globalization.CultureInfo.InvariantCulture);
            var toLon = toCoordinates[0].ToString(System.Globalization.CultureInfo.InvariantCulture);
            var toLat = toCoordinates[1].ToString(System.Globalization.CultureInfo.InvariantCulture);

            var url = $"v2/directions/{transportType}?start={fromLon},{fromLat}&end={toLon},{toLat}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Routing API returned status code {response.StatusCode}");
                    throw new RoutingException($"Routing API returned error {response.StatusCode}: {content}");
                }

                var directions = JsonSerializer.Deserialize<OrsDirectionsResponse>(content, JsonOptions);
                var properties = directions?.Features?.FirstOrDefault()?.Properties;
                if (properties?.Summary is not { } summary)
                {
                    _logger.LogWarning($"Invalid routing API response: No route summary");
                    throw new RoutingException("The routing API response did not contain a valid route summary.");
                }

                return new RouteInfo
                {
                    Distance = Math.Round(summary.Distance) / 1000.0,
                    EstimatedTime = Math.Round(summary.Duration) / 60.0,
                    Directions = FormatDirections(properties.Segments),
                    GeoJson = content
                };
            }
            catch (RoutingException)
            {
                throw;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Genuine caller cancellation (not an HttpClient timeout)
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
                _ => throw new UnsupportedTransportTypeException($"Unsupported transport type: {input}")
            };
        }

        private string FormatDirections(IReadOnlyList<OrsSegment>? segments)
        {
            try
            {
                var steps = segments?.FirstOrDefault()?.Steps;
                if (steps == null || steps.Count == 0)
                {
                    return string.Empty;
                }

                var directionsList = new List<string>();
                foreach (var step in steps)
                {
                    var name = step.Name;
                    var line =
                        $"→ {step.Instruction ?? "Unknown instruction"}" +
                        (string.IsNullOrWhiteSpace(name) || name == "-" ? "" : $" (on {name})") + "\n" +
                        $"   Distance: {step.Distance:F0} m, Duration: {step.Duration:F0} s";

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
