using NSubstitute;
using System.Collections.ObjectModel;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private ITourDataService _mockTourDataService;
        private IUiCoordinator _mockUiCoordinator;
        private ISelectedTourService _mockSelectedTourService;
        private IMapService _mockMapService;
        private TourListViewModel _tourListViewModel;
        private TourInfoViewModel _tourInfoViewModel;
        private TourLogsViewModel _tourLogsViewModel;
        private MenuBarViewModel _menuBarViewModel;
        private SearchViewModel _searchViewModel;
        private MainWindowViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create substitutes for dependencies
            _mockTourDataService = Substitute.For<ITourDataService>();
            _mockUiCoordinator = Substitute.For<IUiCoordinator>();
            _mockSelectedTourService = Substitute.For<ISelectedTourService>();
            _mockMapService = Substitute.For<IMapService>();

            // Setup default returns for TourDataService
            _mockTourDataService.Tours.Returns(new ObservableCollection<Tour>());
            _mockMapService.IsInitialized.Returns(true);

            // Create additional required services for MenuBarViewModel
            var mockTourReportPdfService = Substitute.For<ITourReportPdfService>();
            var mockTourSummaryPdfService = Substitute.For<ITourSummaryPdfService>();
            var mockDialogService = Substitute.For<IDialogService>();
            var mockApplicationService = Substitute.For<IApplicationService>();

            // Create real instances of child view models with mocked dependencies
            _tourListViewModel = new TourListViewModel(_mockTourDataService, _mockUiCoordinator, _mockSelectedTourService);
            _tourInfoViewModel = new TourInfoViewModel(_mockSelectedTourService, _mockMapService);
            _tourLogsViewModel = new TourLogsViewModel(_mockTourDataService, _mockSelectedTourService, _mockUiCoordinator);
            _searchViewModel = new SearchViewModel(_mockTourDataService);

            // Create MenuBarViewModel with all required dependencies
            _menuBarViewModel = new MenuBarViewModel(
                _mockTourDataService,
                mockTourReportPdfService,
                mockTourSummaryPdfService,
                _mockMapService,
                mockDialogService,
                mockApplicationService,
                _mockSelectedTourService,
                _tourListViewModel,
                _tourLogsViewModel
            );

            // Create MainWindowViewModel
            _viewModel = new MainWindowViewModel(
                _mockTourDataService,
                _mockUiCoordinator,
                _mockSelectedTourService,
                _tourListViewModel,
                _tourInfoViewModel,
                _tourLogsViewModel,
                _menuBarViewModel,
                _searchViewModel
            );
        }

        [Test]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Assert
            Assert.That(_viewModel.Tours, Is.EqualTo(_mockTourDataService));
            Assert.That(_viewModel.TourListViewModel, Is.EqualTo(_tourListViewModel));
            Assert.That(_viewModel.TourInfoViewModel, Is.EqualTo(_tourInfoViewModel));
            Assert.That(_viewModel.TourLogsViewModel, Is.EqualTo(_tourLogsViewModel));
            Assert.That(_viewModel.MenuBarViewModel, Is.EqualTo(_menuBarViewModel));
            Assert.That(_viewModel.SearchViewModel, Is.EqualTo(_searchViewModel));
        }

        [Test]
        public void OpenAddNewTourView_ShouldCreateAddNewTourViewModel()
        {
            // Act
            _mockUiCoordinator.RequestOpenAddTourView += Raise.Event<Action>();

            // Assert
            Assert.That(_viewModel.AddNewTourViewModel, Is.Not.Null);
        }

        [Test]
        public void CloseAddNewTourView_ShouldClearAddNewTourViewModel()
        {
            // Arrange
            _mockUiCoordinator.RequestOpenAddTourView += Raise.Event<Action>();

            // Act
            _mockUiCoordinator.RequestCloseAddTourView += Raise.Event<Action>();

            // Assert
            Assert.That(_viewModel.AddNewTourViewModel, Is.Null);
            Assert.That(_tourListViewModel.IsAddingTour, Is.False);
        }

        [Test]
        public void OpenEditTourView_ShouldCreateEditTourViewModel_WhenTourIsSelected()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour", TourLogs = new List<TourLog>() };
            _tourListViewModel.SelectedTour = testTour;

            // Act
            _mockUiCoordinator.RequestOpenEditTourView += Raise.Event<Action>();

            // Assert
            Assert.That(_viewModel.EditTourViewModel, Is.Not.Null);
            Assert.That(_tourListViewModel.IsEditingTour, Is.True);
        }

        [Test]
        public void OpenEditTourLogView_ShouldCreateEditTourLogViewModel_WhenTourLogIsSelected()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour", TourLogs = new List<TourLog>() };
            var testTourLog = new TourLog { Id = Guid.NewGuid(), Comment = "Test Log" };
            _mockSelectedTourService.SelectedTour = testTour;
            _mockSelectedTourService.SelectedTourLog = testTourLog;
            _tourLogsViewModel.SelectedTourLog = testTourLog;

            // Act
            _mockUiCoordinator.RequestOpenEditTourLogView += Raise.Event<Action>();

            // Assert
            Assert.That(_viewModel.EditTourLogViewModel, Is.Not.Null);
            Assert.That(_tourLogsViewModel.IsEditingTourLog, Is.True);
        }
    }
}