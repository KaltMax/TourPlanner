namespace TourPlanner.BLL.Services
{
    // Minimal DTOs mirroring only the parts of the OpenRouteService JSON responses that
    // RouteService actually consumes. Any unmapped fields in the response are ignored by System.Text.Json.

    // Geocoding: GET /geocode/search
    internal sealed record OrsGeocodeResponse(IReadOnlyList<OrsGeocodeFeature>? Features);
    internal sealed record OrsGeocodeFeature(OrsGeometry? Geometry);
    internal sealed record OrsGeometry(double[]? Coordinates);

    // Directions: GET /v2/directions/{profile}
    internal sealed record OrsDirectionsResponse(IReadOnlyList<OrsDirectionsFeature>? Features);
    internal sealed record OrsDirectionsFeature(OrsProperties? Properties);
    internal sealed record OrsProperties(OrsSummary? Summary, IReadOnlyList<OrsSegment>? Segments);
    internal sealed record OrsSummary(double Distance, double Duration);
    internal sealed record OrsSegment(IReadOnlyList<OrsStep>? Steps);
    internal sealed record OrsStep(string? Instruction, string? Name, double Distance, double Duration);
}
