namespace TourPlanner.BLL.Exceptions
{
    public class GeocodingException : Exception
    {
        public GeocodingException() { }

        public GeocodingException(string location)
            : base($"Failed to geocode location: {location}") { }

        public GeocodingException(string message, Exception inner)
            : base(message, inner) { }
    }
}
