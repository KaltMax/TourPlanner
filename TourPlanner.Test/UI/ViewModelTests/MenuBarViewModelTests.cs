using NSubstitute;
using System.Collections.ObjectModel;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class MenuBarViewModelTests
    {
        private ITourDataService _mockTourDataService;
        private ITourReportPdfService _mockTourReportPdfService;
        private ITourSummaryPdfService _mockTourSummaryPdfService;
        private IMapService _mockMapService;
        private IDialogService _mockDialogService;
        private IApplicationService _mockApplicationService;
        private ISelectedTourService _mockSelectedTourService;
        private IUiCoordinator _mockUiCoordinator;
        private TourListViewModel _tourListViewModel;
        private TourLogsViewModel _tourLogsViewModel;
        private MenuBarViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create substitutes for dependencies
            _mockTourDataService = Substitute.For<ITourDataService>();
            _mockTourReportPdfService = Substitute.For<ITourReportPdfService>();
            _mockTourSummaryPdfService = Substitute.For<ITourSummaryPdfService>();
            _mockMapService = Substitute.For<IMapService>();
            _mockDialogService = Substitute.For<IDialogService>();
            _mockApplicationService = Substitute.For<IApplicationService>();
            _mockSelectedTourService = Substitute.For<ISelectedTourService>();
            _mockUiCoordinator = Substitute.For<IUiCoordinator>();

            // Setup default returns
            _mockTourDataService.Tours.Returns(new ObservableCollection<Tour>());
            _mockMapService.IsInitialized.Returns(true);

            // Create real instances of ViewModels with mocked dependencies
            _tourListViewModel = new TourListViewModel(_mockTourDataService, _mockUiCoordinator, _mockSelectedTourService);
            _tourLogsViewModel = new TourLogsViewModel(_mockTourDataService, _mockSelectedTourService, _mockUiCoordinator);

            // Create view model
            _viewModel = new MenuBarViewModel(
                _mockTourDataService,
                _mockTourReportPdfService,
                _mockTourSummaryPdfService,
                _mockMapService,
                _mockDialogService,
                _mockApplicationService,
                _mockSelectedTourService,
                _tourListViewModel,
                _tourLogsViewModel
            );
        }

        [Test]
        public void CanExportTourReport_ShouldReturnTrue_WhenTourIsSelectedAndMapIsInitialized()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour" };
            _mockSelectedTourService.SelectedTour.Returns(testTour);
            _mockMapService.IsInitialized.Returns(true);

            // Act
            bool canExecute = _viewModel.ExportTourReportCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void CanExportTourReport_ShouldReturnFalse_WhenNoTourIsSelected()
        {
            // Arrange
            _mockSelectedTourService.SelectedTour.Returns((Tour?)null);
            _mockMapService.IsInitialized.Returns(true);

            // Act
            bool canExecute = _viewModel.ExportTourReportCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.False);
        }

        [Test]
        public void CanExportTourSummary_ShouldReturnTrue_WhenToursExist()
        {
            // Arrange
            var tours = new ObservableCollection<Tour>
            {
                new Tour { Id = Guid.NewGuid(), Name = "Tour 1" }
            };
            _mockTourDataService.Tours.Returns(tours);

            // Act
            bool canExecute = _viewModel.ExportTourSummaryCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void ExitCommand_ShouldCallApplicationShutdown_WhenUserConfirms()
        {
            // Arrange
            _mockDialogService.ShowConfirmation(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            // Act
            _viewModel.ExitCommand.Execute(null);

            // Assert
            _mockApplicationService.Received(1).Shutdown();
        }

        [Test]
        public void ExitCommand_ShouldNotCallApplicationShutdown_WhenUserCancels()
        {
            // Arrange
            _mockDialogService.ShowConfirmation(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            // Act
            _viewModel.ExitCommand.Execute(null);

            // Assert
            _mockApplicationService.DidNotReceive().Shutdown();
        }

        [Test]
        public void ViewHelpCommand_ShouldShowInformationDialog()
        {
            // Act
            _viewModel.ViewHelpCommand.Execute(null);

            // Assert
            _mockDialogService.Received(1).ShowInformation(
                "Sorry, we can't help you yet. All hope is lost!",
                "Help");
        }
    }
}