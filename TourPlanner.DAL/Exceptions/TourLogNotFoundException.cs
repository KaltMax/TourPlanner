namespace TourPlanner.DAL.Exceptions
{
    public class TourLogNotFoundException : Exception
    {
        public TourLogNotFoundException() { }

        public TourLogNotFoundException(Guid id)
            : base($"TourLog with Id={id} not found.") { }

        public TourLogNotFoundException(string? message)
            : base(message) { }

        public TourLogNotFoundException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
