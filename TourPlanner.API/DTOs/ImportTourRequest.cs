using System.ComponentModel.DataAnnotations;

namespace TourPlanner.API.DTOs
{
    /// <summary>
    /// A tour as supplied to the bulk-import endpoint. Unlike <see cref="TourRequest"/>
    /// </summary>
    public record ImportTourRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; init; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; init; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string From { get; init; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string To { get; init; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string TransportType { get; init; } = string.Empty;

        public string GeoJson { get; init; } = string.Empty;
        public string Directions { get; init; } = string.Empty;
        public double Distance { get; init; }
        public double EstimatedTime { get; init; }

        public List<ImportTourLogRequest> TourLogs { get; init; } = [];
    }

    /// <summary>
    /// A tour log carried inside an <see cref="ImportTourRequest"/>. Ids and the parent
    /// tour reference are regenerated on import, so they are not accepted from the client.
    /// </summary>
    public record ImportTourLogRequest
    {
        public DateTime Date { get; init; }

        [StringLength(1000)]
        public string Comment { get; init; } = string.Empty;

        public double TotalDistance { get; init; }
        public double TotalTime { get; init; }
        public double Difficulty { get; init; }
        public double Rating { get; init; }
    }
}
