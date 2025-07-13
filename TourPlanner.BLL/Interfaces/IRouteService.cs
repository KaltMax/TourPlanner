using TourPlanner.BLL.DomainModels;

namespace TourPlanner.BLL.Interfaces
{
    public interface IRouteService
    {
        Task <RouteInfo> GetRouteInfoAsync(string from, string to, string transportType);
    }
}
