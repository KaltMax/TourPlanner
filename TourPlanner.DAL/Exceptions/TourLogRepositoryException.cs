namespace TourPlanner.DAL.Exceptions
{
    public class TourLogRepositoryException : Exception
    {
        public TourLogRepositoryException() { }

        public TourLogRepositoryException(string? message)
            : base(message) { }

        public TourLogRepositoryException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
