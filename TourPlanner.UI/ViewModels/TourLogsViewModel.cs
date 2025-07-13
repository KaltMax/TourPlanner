using System.Collections.ObjectModel;
using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;

namespace TourPlanner.UI.ViewModels
{
    public class TourLogsViewModel : ViewModelBase
    {
        private readonly ITourDataService _tourDataService;
        private readonly ISelectedTourService _selectionService;
        private readonly IUiCoordinator _uiCoordinator;

        public ObservableCollection<TourLog> TourLogs { get; } = new();

        private TourLog? _selectedTourLog;
        public TourLog? SelectedTourLog
        {
            get => _selectedTourLog;
            set
            {
                if (_selectedTourLog == value)
                    return;

                if (_isEditingTourLog)
                {
                    _uiCoordinator.CloseEditTourLogView();
                    IsEditingTourLog = false;
                }

                _selectedTourLog = value;
                _selectionService.SelectedTourLog = value;
                _selectionService.RaiseSelectedTourLogChanged();

                OnPropertyChanged();
                ((RelayCommand)AddTourLogCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveTourLogCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditTourLogCommand).RaiseCanExecuteChanged();
            }
        }

        private bool _isAddingTourLog;
        public bool IsAddingTourLog
        {
            get => _isAddingTourLog;
            set
            {
                if (_isAddingTourLog != value)
                {
                    _isAddingTourLog = value;
                    OnPropertyChanged();
                    ((RelayCommand)EditTourLogCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isEditingTourLog;
        public bool IsEditingTourLog
        {
            get => _isEditingTourLog;
            set
            {
                if (_isEditingTourLog != value)
                {
                    _isEditingTourLog = value;
                    OnPropertyChanged();
                    ((RelayCommand)AddTourLogCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)RemoveTourLogCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddTourLogCommand { get; }
        public ICommand RemoveTourLogCommand { get; }
        public ICommand EditTourLogCommand { get; }

        public TourLogsViewModel(ITourDataService tourDataService, ISelectedTourService selectionService, IUiCoordinator uiCoordinator)
        {
            _tourDataService = tourDataService;
            _selectionService = selectionService;
            _uiCoordinator = uiCoordinator;

            _selectionService.SelectedTourChanged += OnSelectedTourChanged;

            AddTourLogCommand = new RelayCommand(AddTourLog, CanAddTourLog);
            RemoveTourLogCommand = new RelayCommand(RemoveTourLog, CanRemoveTourLog);
            EditTourLogCommand = new RelayCommand(EditTourLog, CanEditTourLog);

            UpdateTourLogs(_selectionService.SelectedTour);
        }

        public void UpdateTourLogs(Tour? selectedTour)
        {
            TourLogs.Clear();

            if (selectedTour?.TourLogs != null)
            {
                foreach (var log in selectedTour.TourLogs)
                {
                    TourLogs.Add(log);
                }
            }

            OnPropertyChanged(nameof(TourLogs));
        }

        private void OnSelectedTourChanged(object? sender, Tour? newTour)
        {
            if (IsAddingTourLog)
            {
                _uiCoordinator.CloseAddTourLogView();
                IsAddingTourLog = false;
            }

            if (IsEditingTourLog)
            {
                _uiCoordinator.CloseEditTourLogView();
                IsEditingTourLog = false;
            }

            UpdateTourLogs(newTour);

            if (_selectionService.SelectedTourLog != null)
            {
                var selectedId = _selectionService.SelectedTourLog.Id;
                SelectedTourLog = TourLogs.FirstOrDefault(log => log.Id == selectedId);
            }
            else
            {
                SelectedTourLog = null;
            }

            ((RelayCommand)AddTourLogCommand).RaiseCanExecuteChanged();
        }


        private bool CanAddTourLog(object? parameter) => _selectionService.SelectedTour != null && !IsEditingTourLog;

        private void AddTourLog(object? parameter)
        {
            if (_selectionService.SelectedTour == null)
                return;

            _uiCoordinator.OpenAddTourLogView();
        }

        private bool CanRemoveTourLog(object? parameter) => SelectedTourLog != null && !IsEditingTourLog;

        private async void RemoveTourLog(object? parameter)
        {
            if (SelectedTourLog == null || _selectionService.SelectedTour == null)
                return;

            var currentTourId = _selectionService.SelectedTour.Id;
            await _tourDataService.RemoveTourLogAsync(SelectedTourLog);

            var updatedTour = _tourDataService.Tours.FirstOrDefault(t => t.Id == currentTourId);
            if (updatedTour != null)
            {
                _selectionService.SelectedTour = updatedTour;
                _selectionService.RaiseSelectedTourChanged();
                UpdateTourLogs(updatedTour);
            }

            SelectedTourLog = null;
            _selectionService.RaiseSelectedTourLogChanged();
        }

        private bool CanEditTourLog(object? parameter) => SelectedTourLog != null && !IsAddingTourLog;

        private void EditTourLog(object? parameter)
        {
            if (SelectedTourLog == null || _selectionService.SelectedTour == null)
                return;

            _uiCoordinator.OpenEditTourLogView();
        }
    }
}
