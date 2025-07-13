using TourPlanner.DAL.Entities;

namespace TourPlanner.DAL.Interfaces
{
    public interface ITourRepository
    {
        Task<IEnumerable<TourEntity>> GetAllToursAsync();
        Task<TourEntity> AddTourAsync(TourEntity newTourEntity);
        Task<bool> DeleteTourAsync(Guid id);
        Task<TourEntity> UpdateTourAsync(TourEntity tourEntity);
        Task<bool> TourNameExistsAsync(string name, Guid? excludeId = null);
    }
}
