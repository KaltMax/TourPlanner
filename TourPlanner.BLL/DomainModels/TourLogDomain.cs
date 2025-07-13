namespace TourPlanner.BLL.DomainModels
{
    public class TourLogDomain
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; } = string.Empty;
        public double TotalDistance { get; set; } 
        public double TotalTime { get; set; }
        public double Difficulty { get; set; }
        public double Rating { get; set; }
        public Guid TourId { get; set; }

        public TourLogDomain() {}

        public TourLogDomain(Guid id, DateTime date, string comment, double totalDistance, double totalTime, double difficulty, double rating, Guid tourId)
        {
            Id = id;
            Date = date;
            Comment = comment;
            TotalDistance = totalDistance;
            TotalTime = totalTime;
            Difficulty = difficulty;
            Rating = rating;
            TourId = tourId;
        }
    }
}
