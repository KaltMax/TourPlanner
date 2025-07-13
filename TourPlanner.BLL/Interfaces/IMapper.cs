using TourPlanner.BLL.DomainModels;
using TourPlanner.DAL.Entities;

namespace TourPlanner.BLL.Interfaces
{
    public interface IMapper
    {
        TourDomain ToDomain (TourEntity tourEntity);
        TourEntity ToEntity(TourDomain tourDomain);

        TourLogDomain ToDomain(TourLogEntity tourLogEntity);
        TourLogEntity ToEntity(TourLogDomain tourLogDomain);
    }
}
