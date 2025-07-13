namespace TourPlanner.BLL.Exceptions
{
    public class TourServiceException : Exception
    {
        public TourServiceException() { }

        public TourServiceException(string? message)
            : base(message) { }

        public TourServiceException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
