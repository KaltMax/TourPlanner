using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels.Base;

namespace TourPlanner.UI.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly ITourDataService _tourDataService;

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    ((RelayCommand)SearchCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (_isSearching != value)
                {
                    _isSearching = value;
                    OnPropertyChanged();
                    ((RelayCommand)ResetSearchCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand ResetSearchCommand { get; }

        public SearchViewModel(ITourDataService tourDataService)
        {
            _tourDataService = tourDataService;

            // Initialize commands
            SearchCommand = new RelayCommand(ExecuteSearch, CanSearch);
            ResetSearchCommand = new RelayCommand(ExecuteResetSearch, CanResetSearch);
        }

        private void ExecuteSearch(object? parameter)
        {
            _tourDataService.SearchTours(SearchQuery);
            IsSearching = true;
        }

        private bool CanSearch(object? parameter) => !string.IsNullOrWhiteSpace(SearchQuery);

        private void ExecuteResetSearch(object? parameter)
        {
            _tourDataService.ResetSearch();
            SearchQuery = string.Empty;
            IsSearching = false;
        }

        private bool CanResetSearch(object? parameter) => IsSearching;
    }
}