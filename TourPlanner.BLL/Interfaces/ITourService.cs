using TourPlanner.BLL.DomainModels;

namespace TourPlanner.BLL.Interfaces
{
    public interface ITourService
    {
        Task<IEnumerable<TourDomain>> GetAllToursAsync();
        Task<TourDomain> AddTourAsync(TourDomain newTour, CancellationToken cancellationToken = default);
        Task<bool> DeleteTourAsync(Guid id);
        Task<TourDomain> UpdateTourAsync(Guid id, TourDomain updatedTour, CancellationToken cancellationToken = default);
        Task<int> ImportToursAsync(IEnumerable<TourDomain> tours, CancellationToken cancellationToken = default);
    }
}
