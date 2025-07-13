using TourPlanner.DAL.Entities;

namespace TourPlanner.DAL.Interfaces
{
    public interface ITourLogRepository
    {
        Task<TourLogEntity?> GetTourLogByIdAsync(Guid id);
        Task<TourLogEntity> AddLogAsync(TourLogEntity newLog);
        Task<bool> DeleteLogAsync(Guid id);
        Task<TourLogEntity> UpdateTourLogAsync(TourLogEntity updatedTourLogEntity);
    }
}
