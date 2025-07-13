namespace TourPlanner.BLL.DomainModels
{
    public class TourDomain
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string GeoJson { get; set; } = string.Empty;
        public string Directions { get; set; } = string.Empty;
        public double Distance { get; set; }
        public double EstimatedTime { get; set; }
        public string TransportType { get; set; } = string.Empty;
        public List<TourLogDomain> TourLogs { get; set; } = new();

        public TourDomain() { }

        public TourDomain(Guid id, string name, string description, string from, string to, string geoJson,
            string directions, double distance, double estimatedTime, string transportType,
            List<TourLogDomain> tourLogs)
        {
            Id = id;
            Name = name;
            Description = description;
            From = from;
            To = to;
            GeoJson = geoJson;
            Directions = directions;
            Distance = distance;
            EstimatedTime = estimatedTime;
            TransportType = transportType;
            TourLogs = tourLogs;
        }
    }
}
