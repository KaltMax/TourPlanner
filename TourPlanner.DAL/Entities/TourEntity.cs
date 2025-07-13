using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourPlanner.DAL.Entities
{
    public class TourEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string From { get; set; } = string.Empty;

        [Required]
        public string To { get; set; } = string.Empty;

        [Required]
        public string GeoJson { get; set; } = string.Empty;

        [Required]
        public string Directions { get; set; } = string.Empty;

        [Required]
        public double Distance { get; set; }

        [Required]
        public double EstimatedTime { get; set; }

        [Required]
        public string TransportType { get; set; } = string.Empty;

        public ICollection<TourLogEntity> TourLogs { get; set; } = new List<TourLogEntity>();
    }
}