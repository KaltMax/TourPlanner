using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.ViewModels.Base;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.ViewModels
{
    public class AddNewTourLogViewModel : ViewModelBase
    {
        private readonly ITourDataService _tourDataService;
        private readonly ISelectedTourService _selectedTourService;
        private readonly IUiCoordinator _uiCoordinator;

        public DateTime Date { get; set; } = DateTime.Now;

        private string _comment = string.Empty;
        public string Comment
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

        public AddNewTourLogViewModel(ITourDataService tourDataService, ISelectedTourService selectedTour, IUiCoordinator uiCoordinator)
        {
            _tourDataService = tourDataService;
            _selectedTourService = selectedTour;
            _uiCoordinator = uiCoordinator;
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            _selectedTourService.SelectedTourChanged += (sender, tour) => ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool CanSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(Comment) &&
                   _selectedTourService.SelectedTour != null &&
                   Difficulty > 0 &&
                   TotalDistance > 0 &&
                   TotalTime > 0 &&
                   Rating > 0;
        }

        private async void Save(object? parameter)
        {
            if (_selectedTourService.SelectedTour == null) return;

            var currentTourId = _selectedTourService.SelectedTour.Id;

            var newLog = new TourLog
            {
                Date = DateTime.Now,
                Comment = Comment,
                Difficulty = Difficulty,
                TotalDistance = TotalDistance,
                TotalTime = TotalTime,
                Rating = Rating,
                TourId = currentTourId
            };

            await _tourDataService.AddTourLogAsync(newLog);
            var updatedTour = _tourDataService.Tours.FirstOrDefault(t => t.Id == currentTourId);
            _selectedTourService.SelectedTour = updatedTour;
            _uiCoordinator.CloseAddTourLogView();
        }

        private void Cancel(object? parameter)
        {
            _uiCoordinator.CloseAddTourLogView();
        }
    }
}
