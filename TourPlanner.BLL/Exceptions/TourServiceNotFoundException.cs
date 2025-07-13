namespace TourPlanner.BLL.Exceptions
{
    public class TourServiceNotFoundException : Exception
    {
        public TourServiceNotFoundException() { }

        public TourServiceNotFoundException(string? message)
            : base(message) { }

        public TourServiceNotFoundException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
