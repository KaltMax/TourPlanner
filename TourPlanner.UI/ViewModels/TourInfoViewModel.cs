using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;
using Microsoft.Web.WebView2.Wpf;

namespace TourPlanner.UI.ViewModels
{
    public class TourInfoViewModel : ViewModelBase
    {
        private readonly ISelectedTourService _selectedTourService;
        private readonly IMapService _mapService;

        public bool IsMapAvailable => _mapService.IsInitialized;

        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                OnPropertyChanged();
                OnTourChanged();
            }
        }

        public TourInfoViewModel(ISelectedTourService selectedTourService, IMapService mapService)
        {
            _selectedTourService = selectedTourService;
            _mapService = mapService;
            _selectedTourService.SelectedTourChanged += OnSelectedTourChanged;
            SelectedTour = _selectedTourService.SelectedTour;
        }

        public async Task InitializeMapAsync(WebView2 webView)
        {
            _mapService.RegisterWebView(webView);
            await _mapService.InitializeAsync();
            OnPropertyChanged(nameof(IsMapAvailable));
        }

        private void OnSelectedTourChanged(object? sender, Tour? newTour)
        {
            SelectedTour = newTour;
        }

        private async void OnTourChanged()
        {
            try
            {
                if (_mapService.IsInitialized)
                {
                    if (SelectedTour != null && !string.IsNullOrEmpty(SelectedTour.GeoJson))
                    {
                        await _mapService.LoadGeoJsonMapAsync(SelectedTour.GeoJson);
                    }
                    else
                    {
                        await _mapService.ClearMapAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Map error: {ex.Message}");
            }
        }
    }
}