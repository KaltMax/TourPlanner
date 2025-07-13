namespace TourPlanner.BLL.DomainModels
{
    public class RouteInfo
    {
        public string GeoJson { get; set; } = string.Empty;
        public string Directions { get; set; } = string.Empty;
        public double Distance {get; set; }
        public double EstimatedTime { get; set; }
    }
}
