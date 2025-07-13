using System.Collections.ObjectModel;
using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;

namespace TourPlanner.UI.ViewModels
{
    public class TourListViewModel : ViewModelBase
    {
        private readonly ITourDataService _dataService;
        private readonly IUiCoordinator _uiCoordinator;
        private readonly ISelectedTourService _selectedTourService;

        public ObservableCollection<Tour> Tours => _dataService.Tours;

        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                if (_selectedTour == value)
                    return;

                _selectedTour = value;
                OnPropertyChanged();

                _selectedTourService.SelectedTour = _selectedTour;

                if (IsEditingTour)
                {
                    _uiCoordinator.CloseEditTourView();
                    IsEditingTour = false;
                }

                ((RelayCommand)RemoveTourCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditTourCommand).RaiseCanExecuteChanged();
            }
        }

        private bool _isAddingTour;
        public bool IsAddingTour
        {
            get => _isAddingTour;
            set
            {
                if (_isAddingTour != value)
                {
                    _isAddingTour = value;
                    OnPropertyChanged();
                    ((RelayCommand)EditTourCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)RemoveTourCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isEditingTour;
        public bool IsEditingTour
        {
            get => _isEditingTour;
            set
            {
                if (_isEditingTour != value)
                {
                    _isEditingTour = value;
                    OnPropertyChanged();
                    ((RelayCommand)AddTourCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)RemoveTourCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddTourCommand { get; }
        public ICommand RemoveTourCommand { get; }
        public ICommand EditTourCommand { get; }

        public TourListViewModel(
            ITourDataService dataService,
            IUiCoordinator uiCoordinator,
            ISelectedTourService selectedTourService)
        {
            _dataService = dataService;
            _uiCoordinator = uiCoordinator;
            _selectedTourService = selectedTourService;

            _selectedTourService.SelectedTourChanged += OnSelectedTourChanged;

            AddTourCommand = new RelayCommand(AddTour, CanAddTour);
            RemoveTourCommand = new RelayCommand(RemoveTour, CanRemoveTour);
            EditTourCommand = new RelayCommand(EditTour, CanEditTour);
        }

        private bool CanAddTour(object? parameter) => !IsEditingTour;

        private void AddTour(object? parameter)
        {
            if (IsAddingTour)
            {
                _uiCoordinator.CloseAddTourView();
                return;
            }

            IsAddingTour = true;
            _uiCoordinator.OpenAddTourView();
        }

        private bool CanEditTour(object? parameter) =>
            SelectedTour != null && !IsAddingTour && !SelectedTour.TourLogs.Any();

        private void EditTour(object? parameter)
        {

            if (IsEditingTour)
            {
                _uiCoordinator.CloseEditTourView();
                IsEditingTour = false;
                return;
            }

            IsEditingTour = true;
            _uiCoordinator.OpenEditTourView();
        }

        private bool CanRemoveTour(object? parameter) => SelectedTour != null && !IsAddingTour && !IsEditingTour;

        private async void RemoveTour(object? parameter)
        {
            if (SelectedTour is null) return;
            await _dataService.RemoveTourAsync(SelectedTour);
            SelectedTour = null;
        }

        private void OnSelectedTourChanged(object? sender, Tour? newTour)
        {
            SelectedTour = newTour;
            ((RelayCommand)EditTourCommand).RaiseCanExecuteChanged();
        }
    }
}
