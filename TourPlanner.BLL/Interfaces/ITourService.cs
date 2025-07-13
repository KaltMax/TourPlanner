using TourPlanner.BLL.DomainModels;

namespace TourPlanner.BLL.Interfaces
{
    public interface ITourService
    {
        Task<IEnumerable<TourDomain>> GetAllToursAsync();
        Task<TourDomain> AddTourAsync(TourDomain newTour);
        Task<bool> DeleteTourAsync(Guid id);
        Task<TourDomain> UpdateTourAsync(Guid id, TourDomain updatedTour);
        Task<int> ImportToursAsync(IEnumerable<TourDomain> tours);
    }
}
