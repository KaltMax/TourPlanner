using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;

namespace TourPlanner.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUiCoordinator _uiCoordinator;
        private readonly ISelectedTourService _selectedTour;

        public ITourDataService Tours { get; }
        public TourListViewModel TourListViewModel { get; }
        public TourInfoViewModel TourInfoViewModel { get; }
        public TourLogsViewModel TourLogsViewModel { get; }
        public MenuBarViewModel MenuBarViewModel { get; }
        public SearchViewModel SearchViewModel { get; }

        private AddNewTourViewModel? _addNewTourViewModel;
        public AddNewTourViewModel? AddNewTourViewModel
        {
            get => _addNewTourViewModel;
            set
            {
                _addNewTourViewModel = value;
                OnPropertyChanged();
            }
        }

        private AddNewTourLogViewModel? _addNewTourLogViewModel;
        public AddNewTourLogViewModel? AddNewTourLogViewModel
        {
            get => _addNewTourLogViewModel;
            set
            {
                _addNewTourLogViewModel = value;
                OnPropertyChanged();
            }
        }

        private EditTourViewModel? _editTourViewModel;
        public EditTourViewModel? EditTourViewModel
        {
            get => _editTourViewModel;
            set
            {
                _editTourViewModel = value;
                OnPropertyChanged();
            }
        }

        private EditTourLogViewModel? _editTourLogViewModel;
        public EditTourLogViewModel? EditTourLogViewModel
        {
            get => _editTourLogViewModel;
            set
            {
                _editTourLogViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(
            ITourDataService tours,
            IUiCoordinator uiCoordinator,
            ISelectedTourService selectedTour,
            TourListViewModel tourListViewModel,
            TourInfoViewModel tourInfoViewModel,
            TourLogsViewModel tourLogsViewModel,
            MenuBarViewModel menuBarViewModel,
            SearchViewModel searchViewModel)
        {
            Tours = tours;
            _uiCoordinator = uiCoordinator;
            _selectedTour = selectedTour;
            TourListViewModel = tourListViewModel;
            TourInfoViewModel = tourInfoViewModel;
            TourLogsViewModel = tourLogsViewModel;
            MenuBarViewModel = menuBarViewModel;
            SearchViewModel = searchViewModel;

            // Subscribe to UI Coordinator events
            _uiCoordinator.RequestOpenAddTourView += OpenAddNewTourView;
            _uiCoordinator.RequestCloseAddTourView += CloseAddNewTourView;
            _uiCoordinator.RequestOpenAddTourLogView += OpenAddNewTourLogView;
            _uiCoordinator.RequestCloseAddTourLogView += CloseAddNewTourLogView;
            _uiCoordinator.RequestOpenEditTourView += OpenEditTourView;
            _uiCoordinator.RequestCloseEditTourView += CloseEditTourView;
            _uiCoordinator.RequestOpenEditTourLogView += OpenEditTourLogView;
            _uiCoordinator.RequestCloseEditTourLogView += CloseEditTourLogView;
        }

        private void OpenAddNewTourView()
        {
            if (AddNewTourViewModel == null)
            {
                AddNewTourViewModel = new AddNewTourViewModel(Tours, _selectedTour, _uiCoordinator);
            }
        }

        private void CloseAddNewTourView()
        {
            TourListViewModel.IsAddingTour = false;
            AddNewTourViewModel = null;
        }

        private void OpenAddNewTourLogView()
        {
            if (AddNewTourLogViewModel != null)
            {
                _uiCoordinator.CloseAddTourLogView();
                return;
            }

            if (TourListViewModel.SelectedTour == null) return;

            AddNewTourLogViewModel = new AddNewTourLogViewModel(Tours, _selectedTour, _uiCoordinator);
            TourLogsViewModel.IsAddingTourLog = true;
        }

        private void CloseAddNewTourLogView()
        {
            TourLogsViewModel.IsAddingTourLog = false;
            AddNewTourLogViewModel = null;

            TourLogsViewModel.UpdateTourLogs(TourListViewModel.SelectedTour);
            TourLogsViewModel.OnPropertyChanged(nameof(TourLogsViewModel.TourLogs));
        }

        private void OpenEditTourView()
        {
            if (EditTourViewModel != null)
            {
                _uiCoordinator.CloseEditTourView();
                return;
            }

            if (TourListViewModel.SelectedTour == null) return;

            EditTourViewModel = new EditTourViewModel(Tours, _selectedTour, _uiCoordinator);
            TourListViewModel.IsEditingTour = true;
        }

        private void CloseEditTourView()
        {
            TourListViewModel.IsEditingTour = false;
            EditTourViewModel = null;
        }

        private void OpenEditTourLogView()
        {
            if (EditTourLogViewModel != null)
            {
                _uiCoordinator.CloseEditTourLogView();
                return;
            }

            if (TourLogsViewModel.SelectedTourLog == null) return;

            _selectedTour.SelectedTourLog = TourLogsViewModel.SelectedTourLog;
            _selectedTour.RaiseSelectedTourLogChanged();

            EditTourLogViewModel = new EditTourLogViewModel(Tours, _selectedTour, _uiCoordinator);
            TourLogsViewModel.IsEditingTourLog = true;
        }

        private void CloseEditTourLogView()
        {
            TourLogsViewModel.IsEditingTourLog = false;
            EditTourLogViewModel = null;
        }
    }
}