namespace TourPlanner.BLL.Exceptions
{
    public class TourLogServiceException : Exception
    {
        public TourLogServiceException() { }

        public TourLogServiceException(string? message) : base(message) { }

        public TourLogServiceException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
