namespace TourPlanner.UI.Models
{
    public class TourLog
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string? Comment { get; set; }
        public double TotalDistance { get; set; }
        public double TotalTime { get; set; }
        public double Difficulty { get; set; } // 1 = Very Easy, 2 = Easy, 3 = Medium 4 = Hard 5 = Very Hard;
        public double Rating { get; set; }
        public Guid TourId { get; set; }

        public TourLog()
        {
            Date = DateTime.Now;
            Comment = string.Empty;
            TotalDistance = 0;
            TotalTime = 0;
            Difficulty = 0;
            Rating = 0;
        }
    }
}
