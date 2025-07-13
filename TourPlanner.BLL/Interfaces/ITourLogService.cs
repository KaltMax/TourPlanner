using TourPlanner.BLL.DomainModels;

namespace TourPlanner.BLL.Interfaces
{
    public interface ITourLogService
    {
        Task<TourLogDomain?> GetTourLogByIdAsync(Guid id);
        Task<TourLogDomain> AddLogAsync(TourLogDomain newLog);
        Task<bool> DeleteLogAsync(Guid id);
        Task<TourLogDomain> UpdateTourLogAsync(Guid id, TourLogDomain updatedTourLog);
    }
}
