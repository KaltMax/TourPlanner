namespace TourPlanner.BLL.Exceptions
{
    public class RoutingException : Exception
    {
        public RoutingException() { }

        public RoutingException(string message)
            : base(message) { }

        public RoutingException(string message, Exception inner)
            : base(message, inner) { }
    }
}