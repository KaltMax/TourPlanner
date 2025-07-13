namespace TourPlanner.BLL.Exceptions
{
    public class TourNameAlreadyExistsException : Exception
    {
        public TourNameAlreadyExistsException() { }

        public TourNameAlreadyExistsException(string? message)
            : base(message) { }

        public TourNameAlreadyExistsException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
