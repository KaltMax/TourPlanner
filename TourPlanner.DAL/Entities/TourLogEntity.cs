using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourPlanner.DAL.Entities
{
    public class TourLogEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Comment { get; set; } = string.Empty;

        [Required]
        public double TotalDistance { get; set; }

        [Required]
        public double TotalTime { get; set; }

        [Required]
        public double Difficulty { get; set; }

        [Required]
        public double Rating { get; set; }

        [ForeignKey("Tour")]
        public Guid TourId { get; set; }
        public TourEntity Tour { get; set; } = null!;
    }
}
