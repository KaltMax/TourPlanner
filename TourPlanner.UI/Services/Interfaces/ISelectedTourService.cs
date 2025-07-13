using TourPlanner.UI.Models;

namespace TourPlanner.UI.Services.Interfaces
{
    public interface ISelectedTourService
    {
        Tour? SelectedTour { get; set; }
        event EventHandler<Tour?>? SelectedTourChanged;
        void RaiseSelectedTourChanged();

        TourLog? SelectedTourLog { get; set; }
        event EventHandler<TourLog?>? SelectedTourLogChanged; 

        void RaiseSelectedTourLogChanged();
    }
}
