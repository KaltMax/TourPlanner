using TourPlanner.UI.Models;

namespace TourPlanner.UI.Services.Interfaces
{
    public interface ITourReportPdfService
    {
        Task<bool> GenerateTourReportPdfAsync(Tour tour);
    }
}
