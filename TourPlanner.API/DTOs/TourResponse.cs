namespace TourPlanner.API.DTOs
{
    /// <summary>
    /// The representation of a tour returned to clients
    /// </summary>
    public record TourResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string From { get; init; } = string.Empty;
        public string To { get; init; } = string.Empty;
        public string TransportType { get; init; } = string.Empty;
        public double Distance { get; init; }
        public double EstimatedTime { get; init; }
        public string Directions { get; init; } = string.Empty;
        public string GeoJson { get; init; } = string.Empty;
        public IReadOnlyList<TourLogResponse> TourLogs { get; init; } = [];
    }
}
