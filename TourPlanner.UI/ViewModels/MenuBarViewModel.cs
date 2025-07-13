using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;

namespace TourPlanner.UI.ViewModels
{
    public class MenuBarViewModel : ViewModelBase
    {
        private readonly ITourDataService _tourDataService;
        private readonly ITourReportPdfService _tourReportPdfService;
        private readonly ITourSummaryPdfService _tourSummaryPdfService;
        private readonly IMapService _mapService;
        private readonly IDialogService _dialogService;
        private readonly IApplicationService _applicationService;
        private readonly ISelectedTourService _selectedTourService;

        // Menu Commands
        public ICommand ExitCommand { get; }
        public ICommand ExportTourReportCommand { get; }
        public ICommand ExportTourSummaryCommand { get; }
        public ICommand ReloadToursFromDbCommand { get; }
        public ICommand ViewHelpCommand { get; }
        public ICommand ExportToursCommand { get; }
        public ICommand ImportToursCommand { get; }

        // Action Commands (delegated from other ViewModels)
        public ICommand AddTourCommand { get; }
        public ICommand DeleteTourCommand { get; }
        public ICommand EditTourCommand { get; }
        public ICommand AddTourLogCommand { get; }
        public ICommand DeleteTourLogCommand { get; }
        public ICommand EditTourLogCommand { get; }

        public MenuBarViewModel(
            ITourDataService tourDataService,
            ITourReportPdfService tourReportPdfService,
            ITourSummaryPdfService tourSummaryPdfService,
            IMapService mapService,
            IDialogService dialogService,
            IApplicationService applicationService,
            ISelectedTourService selectedTourService,
            TourListViewModel tourListViewModel,
            TourLogsViewModel tourLogsViewModel)
        {
            _tourDataService = tourDataService;
            _tourReportPdfService = tourReportPdfService;
            _tourSummaryPdfService = tourSummaryPdfService;
            _mapService = mapService;
            _dialogService = dialogService;
            _applicationService = applicationService;
            _selectedTourService = selectedTourService;

            // Initialize menu commands
            ExitCommand = new RelayCommand(ExecuteExit);
            ExportTourReportCommand = new RelayCommand(ExecuteExportTourReport, CanExportTourReport);
            ExportTourSummaryCommand = new RelayCommand(ExecuteExportTourSummary, CanExecuteTourSummary);
            ReloadToursFromDbCommand = new RelayCommand(ExecuteReloadToursFromDb);
            ViewHelpCommand = new RelayCommand(ExecuteViewHelp);
            ExportToursCommand = new RelayCommand(ExecuteExportTours, CanExportTours);
            ImportToursCommand = new RelayCommand(ExecuteImportTours);

            // Get action commands from other ViewModels
            AddTourCommand = tourListViewModel.AddTourCommand;
            DeleteTourCommand = tourListViewModel.RemoveTourCommand;
            EditTourCommand = tourListViewModel.EditTourCommand;
            AddTourLogCommand = tourLogsViewModel.AddTourLogCommand;
            DeleteTourLogCommand = tourLogsViewModel.RemoveTourLogCommand;
            EditTourLogCommand = tourLogsViewModel.EditTourLogCommand;

            // Subscribe to events for command state updates
            _selectedTourService.SelectedTourChanged += (sender, tour) =>
            {
                ((RelayCommand)ExportTourReportCommand).RaiseCanExecuteChanged();
            };

            _tourDataService.Tours.CollectionChanged += (sender, args) =>
            {
                ((RelayCommand)ExportTourSummaryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExportToursCommand).RaiseCanExecuteChanged();
            };
        }

        private bool CanExportTourReport(object? parameter)
        {
            return _selectedTourService.SelectedTour != null && _mapService.IsInitialized;
        }

        private async void ExecuteExportTourReport(object? parameter)
        {
            if (_selectedTourService.SelectedTour != null && _mapService.IsInitialized)
            {
                try
                {
                    // Reload the Map to ensure the route is centered and visible for the screenshot
                    await ReloadMapForSelectedTour();

                    bool success = await _tourReportPdfService.GenerateTourReportPdfAsync(_selectedTourService.SelectedTour);

                    if (success)
                    {
                        _dialogService.ShowInformation("Tour Report PDF was successfully exported.",
                            "Export Successful");
                    }
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error exporting tour report: {ex.Message}", "Export Error");
                }
            }
        }

        private async Task ReloadMapForSelectedTour()
        {
            if (_selectedTourService.SelectedTour != null && !string.IsNullOrEmpty(_selectedTourService.SelectedTour.GeoJson))
            {
                await _mapService.LoadGeoJsonMapAsync(_selectedTourService.SelectedTour.GeoJson);
            }
        }

        private bool CanExecuteTourSummary(object? parameter)
        {
            return _tourDataService.Tours.Any();
        }

        private async void ExecuteExportTourSummary(object? parameter)
        {
            try
            {
                bool success = await _tourSummaryPdfService.GenerateTourSummaryPdfAsync(_tourDataService.Tours);

                if (success)
                {
                    _dialogService.ShowInformation("Tour summary PDF was successfully exported.",
                        "Export Successful");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error exporting tour summary: {ex.Message}",
                    "Export Error");
            }
        }

        private void ExecuteExit(object? parameter)
        {
            bool shouldExit = _dialogService.ShowConfirmation(
                "Are you sure you want to exit the application?",
                "Confirm Exit");

            if (shouldExit)
            {
                _applicationService.Shutdown();
            }
        }

        private async void ExecuteReloadToursFromDb(object? parameter)
        {
            await _tourDataService.LoadToursAsync();
        }

        private void ExecuteViewHelp(object? parameter)
        {
            _dialogService.ShowInformation("Sorry, we can't help you yet. All hope is lost!",
                "Help");
        }

        private bool CanExportTours(object? parameter)
        {
            return _tourDataService.Tours.Any();
        }

        private async void ExecuteExportTours(object? parameter)
        {
            try
            {
                var success = await _tourDataService.ExportToursAsync();
                if (success)
                {
                    _dialogService.ShowInformation("Tours exported successfully!", "Export Complete");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error exporting tours: {ex.Message}", "Export Error");
            }
        }

        private async void ExecuteImportTours(object? parameter)
        {
            try
            {
                var importedTours = await _tourDataService.ImportToursAsync();
                if (importedTours)
                {
                    _dialogService.ShowInformation("Tours successfully imported", "Import Complete");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error importing tours: {ex.Message}", "Import Error");
            }
        }
    }
}