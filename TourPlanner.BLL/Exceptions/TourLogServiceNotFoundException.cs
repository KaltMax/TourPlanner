namespace TourPlanner.BLL.Exceptions
{
    public class TourLogServiceNotFoundException : Exception
    {
        public TourLogServiceNotFoundException() { }

        public TourLogServiceNotFoundException(string? message)
            : base(message) { }

        public TourLogServiceNotFoundException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
