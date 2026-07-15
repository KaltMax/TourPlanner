namespace TourPlanner.BLL.Exceptions
{
    public class UnsupportedTransportTypeException : Exception
    {
        public UnsupportedTransportTypeException() { }

        public UnsupportedTransportTypeException(string? message)
            : base(message) { }

        public UnsupportedTransportTypeException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
