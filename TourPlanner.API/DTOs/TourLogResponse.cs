namespace TourPlanner.API.DTOs
{
    public record TourLogResponse
    {
        public Guid Id { get; init; }
        public DateTime Date { get; init; }
        public string Comment { get; init; } = string.Empty;
        public double TotalDistance { get; init; }
        public double TotalTime { get; init; }
        public double Difficulty { get; init; }
        public double Rating { get; init; }
        public Guid TourId { get; init; }
    }
}
