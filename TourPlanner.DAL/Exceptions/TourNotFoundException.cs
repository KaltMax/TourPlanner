namespace TourPlanner.DAL.Exceptions
{
    public class TourNotFoundException : Exception
    {

        public TourNotFoundException() { }

        public TourNotFoundException(Guid id)
            : base($"Tour with Id={id} not found.") { }

        public TourNotFoundException(string? message)
            : base(message) { }

        public TourNotFoundException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
