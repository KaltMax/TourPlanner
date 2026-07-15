using System.ComponentModel.DataAnnotations;

namespace TourPlanner.API.DTOs
{
    /// <summary>
    /// Client-supplied fields for creating or updating a tour.
    /// </summary>
    public record TourRequest
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
    }
}
