using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Models;
using TourPlanner.UI.ViewModels.Base;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.ViewModels
{
    public class AddNewTourViewModel : ViewModelBase
    {
        private readonly ITourDataService _tours;
        private readonly ISelectedTourService _selectedTourService;
        private readonly IUiCoordinator _uiCoordinator;

        private string _tourName = string.Empty;
        public string TourName
        {
            get => _tourName;
            set
            {
                if (_tourName != value)
                {
                    _tourName = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _from = string.Empty;
        public string From
        {
            get => _from;
            set
            {
                if (_from != value)
                {
                    _from = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _to = string.Empty;
        public string To
        {
            get => _to;
            set
            {
                if (_to != value)
                {
                    _to = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _transportType = string.Empty;
        public string TransportType
        {
            get => _transportType;
            set
            {
                if (_transportType != value)
                {
                    _transportType = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddNewTourViewModel(ITourDataService tours, ISelectedTourService selectedTourService, IUiCoordinator uiCoordinator)
        {
            _tours = tours;
            _selectedTourService = selectedTourService;
            _uiCoordinator = uiCoordinator;
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(TourName) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   !string.IsNullOrWhiteSpace(From) &&
                   !string.IsNullOrWhiteSpace(To) &&
                   !string.IsNullOrWhiteSpace(TransportType);
        }

        private async void Save(object? parameter)
        {
            var newTour = new Tour
            {
                Name = TourName,
                Description = Description,
                From = From,
                To = To,
                TransportType = TransportType,
                Distance = 0,
                EstimatedTime = 0
            };

            var createdTour = await _tours.AddTourAsync(newTour);
            if (createdTour != null)
            {
                var newItem = _tours.Tours.FirstOrDefault(t => t.Id == createdTour.Id);

                _selectedTourService.SelectedTour = newItem;
            }

            _uiCoordinator.CloseAddTourView();
        }

        private void Cancel(object? parameter)
        {
            _uiCoordinator.CloseAddTourView();
        }
    }
}
