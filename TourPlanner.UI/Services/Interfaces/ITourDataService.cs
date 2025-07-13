using System.Collections.ObjectModel;
using TourPlanner.UI.Models;

namespace TourPlanner.UI.Services.Interfaces
{
    public interface ITourDataService
    {
        ObservableCollection<Tour> Tours { get; }

        Task LoadToursAsync();
        Task<Tour?> AddTourAsync(Tour newTour);
        Task RemoveTourAsync(Tour tour);
        Task UpdateTourAsync(Tour tour);

        Task<TourLog?> AddTourLogAsync(TourLog newTourLog);
        Task RemoveTourLogAsync(TourLog tourLog);
        Task UpdateTourLogAsync(TourLog tourLog);

        public void ResetSearch();
        public void SearchTours(string query);
        public void RefreshTours();

        Task<bool> ExportToursAsync();
        Task<bool> ImportToursAsync();
    }
}
