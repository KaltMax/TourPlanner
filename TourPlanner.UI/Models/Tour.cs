using System.Text.Json.Serialization;

namespace TourPlanner.UI.Models
{
    public class Tour
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string GeoJson { get; set; }
        public string Directions { get; set; }
        public double Distance { get; set; }
        public double EstimatedTime { get; set; }
        public string TransportType { get; set; }
        public List<TourLog> TourLogs { get; set; }

        [JsonIgnore]
        public string Popularity
        {
            get => TourLogs.Count switch
            {
                0 => "Not popular",
                < 3 => "Less popular",
                < 5 => "Popular",
                _ => "Very popular"
            };
        }

        // 1 = Very Bad, 2 = Bad, 3 = Medium 4 = Good 5 = Very Good, */
        [JsonIgnore]
        public double AverageRating => TourLogs.Any() ? TourLogs.Average(log => log.Rating) : 0;

        // 1 = Very Easy, 2 = Easy, 3 = Medium 4 = Hard 5 = Very Hard; ChildFriendly  <= 2
        [JsonIgnore]
        public bool IsChildFriendly => !TourLogs.Any() || TourLogs.Average(log => log.Difficulty) <= 2;

        public Tour()
        {
            Name = string.Empty;
            Description = string.Empty;
            From = string.Empty;
            To = string.Empty;
            TransportType = string.Empty;
            GeoJson = string.Empty;
            Directions = string.Empty;
            TourLogs = new List<TourLog>();
        }
    }
}
