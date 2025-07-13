using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;

namespace TourPlanner.UI.ViewModels
{
    public class EditTourViewModel : ViewModelBase
    {
        private readonly ITourDataService _tours;
        private readonly ISelectedTourService? _selectedTour;
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

        public EditTourViewModel(ITourDataService tours, ISelectedTourService? selectedTour, IUiCoordinator uiCoordinator)
        {
            _tours = tours;
            _selectedTour = selectedTour;
            _uiCoordinator = uiCoordinator;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            if (selectedTour?.SelectedTour == null)
            {
                throw new ArgumentNullException(nameof(selectedTour));
            }

            TourName = selectedTour.SelectedTour.Name;
            Description = selectedTour.SelectedTour.Description;
            From = selectedTour.SelectedTour.From;
            To = selectedTour.SelectedTour.To;
            TransportType = selectedTour.SelectedTour.TransportType;
        }

        private bool CanSave(object? parameter)
        {
            return _selectedTour?.SelectedTour != null &&
                   (!string.IsNullOrWhiteSpace(TourName) &&
                    !string.IsNullOrWhiteSpace(Description) &&
                    !string.IsNullOrWhiteSpace(From) &&
                    !string.IsNullOrWhiteSpace(To) &&
                    !string.IsNullOrWhiteSpace(TransportType)) &&
                   (_selectedTour.SelectedTour.Name != TourName ||
                    _selectedTour.SelectedTour.Description != Description ||
                    _selectedTour.SelectedTour.From != From ||
                    _selectedTour.SelectedTour.To != To ||
                    _selectedTour.SelectedTour.TransportType != TransportType);
        }

        private async void Save(object? parameter)
        {
            if (_selectedTour?.SelectedTour == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var editedTourId = _selectedTour.SelectedTour.Id;

            _selectedTour.SelectedTour.Name = TourName;
            _selectedTour.SelectedTour.Description = Description;
            _selectedTour.SelectedTour.From = From;
            _selectedTour.SelectedTour.To = To;
            _selectedTour.SelectedTour.TransportType = TransportType;

            await _tours.UpdateTourAsync(_selectedTour.SelectedTour);

            var updatedItem = _tours.Tours.FirstOrDefault(t => t.Id == editedTourId);

            _selectedTour.SelectedTour = updatedItem;
            _selectedTour.RaiseSelectedTourChanged();
            _uiCoordinator.CloseEditTourView();
        }

        private void Cancel(object? parameter)
        {
            _uiCoordinator.CloseEditTourView();
        }
    }
}
