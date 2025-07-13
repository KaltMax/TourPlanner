namespace TourPlanner.DAL.Exceptions
{
    public class TourRepositoryException : Exception
    {
        public TourRepositoryException() { }

        public TourRepositoryException(string? message)
            : base(message) { }

        public TourRepositoryException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
