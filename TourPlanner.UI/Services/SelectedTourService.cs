using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class SelectedTourService : ISelectedTourService
    {
        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                if (_selectedTour != value)
                {
                    _selectedTour = value;
                    SelectedTourChanged?.Invoke(this, _selectedTour);
                }
            }
        }

        private TourLog? _selectedTourLog;
        public TourLog? SelectedTourLog
        {
            get => _selectedTourLog;
            set
            {
                if (_selectedTourLog != value)
                {
                    _selectedTourLog = value;
                    SelectedTourLogChanged?.Invoke(this, _selectedTourLog);
                }
            }
        }

        public event EventHandler<Tour?>? SelectedTourChanged;

        public void RaiseSelectedTourChanged()
        {
            SelectedTourChanged?.Invoke(this, _selectedTour);
        }

        public event EventHandler<TourLog?>? SelectedTourLogChanged;

        public void RaiseSelectedTourLogChanged()
        {
            SelectedTourLogChanged?.Invoke(this, _selectedTourLog);
        }
    }
}
