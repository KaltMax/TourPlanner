using TourPlanner.UI.Models;

namespace TourPlanner.UI.Services.Interfaces
{
    public interface ITourSummaryPdfService
    {
        Task<bool> GenerateTourSummaryPdfAsync(IEnumerable<Tour> tours);
    }
}
