using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Data;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class TourDataService : ITourDataService
    {
        private readonly IHttpService _httpService;
        private readonly IDialogService _dialogService;
        private readonly IFileDialogService _fileDialogService;
        private string? _lastQuery;

        public ObservableCollection<Tour> Tours { get; }

        private List<Tour> _allTours = new();

        public TourDataService(IHttpService httpService, IDialogService dialogService, IFileDialogService fileDialogService)
        {
            _httpService = httpService;
            _dialogService = dialogService;
            _fileDialogService = fileDialogService;
            Tours = new ObservableCollection<Tour>();
        }

        public async Task LoadToursAsync()
        {
            try
            {
                var toursFromApi = await _httpService.GetAsync<List<Tour>>("Tour");

                if (toursFromApi is not null)
                {
                    _allTours = toursFromApi;

                    if (!string.IsNullOrWhiteSpace(_lastQuery))
                    {
                        SearchTours(_lastQuery);
                    }
                    else
                    {
                        Tours.Clear();
                        foreach (var tour in toursFromApi)
                        {
                            Tours.Add(tour);
                        }
                        RefreshTours();
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Could not load tours at startup.\n\n{ex.Message}", "Connection Error");
            }
        }

        public async Task<Tour?> AddTourAsync(Tour newTour)
        {
            try
            {
                var createdTour = await _httpService.PostAsync("Tour", newTour);
                if (createdTour != null)
                {
                    await LoadToursAsync();
                }
                return createdTour;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to add the tour.\n\n{ex.Message}", "Error");
                return null;
            }
        }

        public async Task RemoveTourAsync(Tour tour)
        {
            try
            {
                await _httpService.DeleteAsync($"Tour/{tour.Id}");
                await LoadToursAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to remove the tour.\n\n{ex.Message}", "Error");
            }
        }

        public async Task UpdateTourAsync(Tour tour)
        {
            try
            {
                await _httpService.PutAsync($"Tour/{tour.Id}", tour);
                await LoadToursAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to update the tour.\n\n{ex.Message}", "Error");
            }
        }

        public async Task<TourLog?> AddTourLogAsync(TourLog newLog)
        {
            try
            {
                var createdLog = await _httpService.PostAsync("TourLog", newLog);
                if (createdLog != null)
                {
                    await LoadToursAsync();
                }
                return createdLog;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to add the tour log.\n\n{ex.Message}", "Error");
                return null;
            }
        }

        public async Task RemoveTourLogAsync(TourLog tourLog)
        {
            try
            {
                await _httpService.DeleteAsync($"TourLog/{tourLog.Id}");
                await LoadToursAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to remove the tour log.\n\n{ex.Message}", "Error");
            }
        }

        public async Task UpdateTourLogAsync(TourLog tourlog)
        {
            try
            {
                await _httpService.PutAsync($"TourLog/{tourlog.Id}", tourlog);
                await LoadToursAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to update the tour log.\n\n{ex.Message}", "Error");
            }
        }

        public void SearchTours(string query)
        {
            _lastQuery = query;
            var lowerQuery = query.ToLower();

            var filtered = _allTours.Where(t =>
                t.Name.ToLower().Contains(lowerQuery) ||
                t.Description.ToLower().Contains(lowerQuery) ||
                t.From.ToLower().Contains(lowerQuery) ||
                t.To.ToLower().Contains(lowerQuery) ||
                t.Popularity.ToLower().Contains(lowerQuery) ||
                t.TourLogs.Any(log =>
                    log.Comment != null && log.Comment.ToLower().Contains(lowerQuery)
                )
            ).ToList();

            Tours.Clear();
            foreach (var tour in filtered)
            {
                Tours.Add(tour);
            }

            RefreshTours();
        }

        public void ResetSearch()
        {
            _lastQuery = null;
            Tours.Clear();
            foreach (var tour in _allTours)
            {
                Tours.Add(tour);
            }

            RefreshTours();
        }

        public void RefreshTours()
        {
            CollectionViewSource.GetDefaultView(Tours).Refresh();
        }

        public async Task<bool> ImportToursAsync()
        {
            var filePath = await _fileDialogService.OpenFileDialogAsync(
                "Import Tours",
                "JSON files (*.json)|*.json|All files (*.*)|*.*"
            );

            if (string.IsNullOrEmpty(filePath))
            {
                return false; // User cancelled
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);

            var importCount = await _httpService.PostRawJsonAsync("Tour/import", jsonContent);

            await LoadToursAsync();

            return true;
        }

        public async Task<bool> ExportToursAsync()
        {
            var filePath = await _fileDialogService.SaveFileDialogAsync(
                "tours_export.json",
                "JSON files (*.json)|*.json|All files (*.*)|*.*",
                "json"
            );

            if (string.IsNullOrEmpty(filePath))
            {
                return false; // User cancelled
            }

            var jsonContent = await _httpService.GetRawJsonAsync("Tour");
            await File.WriteAllTextAsync(filePath, jsonContent);

            return true;
        }
    }
}
