using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;

namespace TourPlanner.UI.ViewModels
{
    public class EditTourLogViewModel : ViewModelBase
    {
        private readonly ITourDataService _tourDataService;
        private readonly ISelectedTourService _selectedTour;
        private readonly IUiCoordinator _uiCoordinator;
        private readonly TourLog _originalTourLog;

        private string? _comment;
        public string? Comment
        {
            get => _comment;
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private double _totalDistance;
        public double TotalDistance
        {
            get => _totalDistance;
            set
            {
                if (_totalDistance != value)
                {
                    _totalDistance = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private double _totalTime;
        public double TotalTime
        {
            get => _totalTime;
            set
            {
                if (_totalTime != value)
                {
                    _totalTime = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private double _difficulty;
        public double Difficulty
        {
            get => _difficulty;
            set
            {
                if (_difficulty != value)
                {
                    _difficulty = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private double _rating;
        public double Rating
        {
            get => _rating;
            set
            {
                if (_rating != value)
                {
                    _rating = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditTourLogViewModel(ITourDataService tourDataService, ISelectedTourService selectedTour, IUiCoordinator uiCoordinator)
        {
            _tourDataService = tourDataService;
            _selectedTour = selectedTour;
            _uiCoordinator = uiCoordinator;

            if (_selectedTour.SelectedTour == null || _selectedTour.SelectedTourLog == null)
            {
                throw new InvalidOperationException("No tour or tour log selected for editing.");
            }

            _originalTourLog = _selectedTour.SelectedTourLog;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            Comment = _originalTourLog.Comment;
            TotalDistance = _originalTourLog.TotalDistance;
            TotalTime = _originalTourLog.TotalTime;
            Difficulty = _originalTourLog.Difficulty;
            Rating = _originalTourLog.Rating;
        }

        private bool CanSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(Comment) &&
                   TotalDistance > 0 &&
                   TotalTime > 0 &&
                   Difficulty > 0 &&
                   Rating > 0 &&
                   (Comment != _originalTourLog.Comment ||
                    TotalDistance != _originalTourLog.TotalDistance ||
                    TotalTime != _originalTourLog.TotalTime ||
                    Difficulty != _originalTourLog.Difficulty ||
                    Rating != _originalTourLog.Rating);
        }

        private async void Save(object? parameter)
        {
            if (_selectedTour.SelectedTour == null || _originalTourLog == null)
                return;

            var currentTourId = _selectedTour.SelectedTour.Id;
            var currentLogId = _originalTourLog.Id;

            _originalTourLog.Comment = Comment;
            _originalTourLog.TotalDistance = TotalDistance;
            _originalTourLog.TotalTime = TotalTime;
            _originalTourLog.Difficulty = Difficulty;
            _originalTourLog.Rating = Rating;

            await _tourDataService.UpdateTourLogAsync(_originalTourLog);

            var updatedTour = _tourDataService.Tours.FirstOrDefault(t => t.Id == currentTourId);
            if (updatedTour != null)
            {
                _selectedTour.SelectedTour = updatedTour;
                var updatedLog = updatedTour.TourLogs.FirstOrDefault(l => l.Id == currentLogId);
                _selectedTour.SelectedTourLog = updatedLog;
                _selectedTour.RaiseSelectedTourChanged();
                _selectedTour.RaiseSelectedTourLogChanged();
            }

            _uiCoordinator.CloseEditTourLogView();
        }

        private void Cancel(object? parameter)
        {
            _uiCoordinator.CloseEditTourLogView();
        }
    }
}
