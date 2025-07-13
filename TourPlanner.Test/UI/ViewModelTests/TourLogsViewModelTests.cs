using NSubstitute;
using System.Collections.ObjectModel;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class TourLogsViewModelTests
    {
        private ITourDataService _mockTourDataService;
        private ISelectedTourService _mockSelectionService;
        private IUiCoordinator _mockUiCoordinator;
        private TourLogsViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create substitutes for dependencies
            _mockTourDataService = Substitute.For<ITourDataService>();
            _mockSelectionService = Substitute.For<ISelectedTourService>();
            _mockUiCoordinator = Substitute.For<IUiCoordinator>();

            // Setup default returns
            _mockTourDataService.Tours.Returns(new ObservableCollection<Tour>());
            _mockSelectionService.SelectedTour.Returns((Tour?)null);

            // Create view model
            _viewModel = new TourLogsViewModel(_mockTourDataService, _mockSelectionService, _mockUiCoordinator);
        }

        [Test]
        public void SelectedTourLog_ShouldUpdateSelectionService_WhenSet()
        {
            // Arrange
            var testTourLog = new TourLog { Id = Guid.NewGuid(), Comment = "Test Log" };

            // Act
            _viewModel.SelectedTourLog = testTourLog;

            // Assert
            _mockSelectionService.Received(1).SelectedTourLog = testTourLog;
            _mockSelectionService.Received(1).RaiseSelectedTourLogChanged();
        }

        [Test]
        public void CanAddTourLog_ShouldReturnTrue_WhenTourIsSelectedAndNotEditing()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour" };
            _mockSelectionService.SelectedTour.Returns(testTour);
            _viewModel.IsEditingTourLog = false;

            // Act
            bool canExecute = _viewModel.AddTourLogCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void CanAddTourLog_ShouldReturnFalse_WhenEditingTourLog()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour" };
            _mockSelectionService.SelectedTour.Returns(testTour);
            _viewModel.IsEditingTourLog = true;

            // Act
            bool canExecute = _viewModel.AddTourLogCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.False);
        }

        [Test]
        public void CanRemoveTourLog_ShouldReturnTrue_WhenTourLogIsSelected()
        {
            // Arrange
            var testTourLog = new TourLog { Id = Guid.NewGuid(), Comment = "Test Log" };
            _viewModel.SelectedTourLog = testTourLog;

            // Act
            bool canExecute = _viewModel.RemoveTourLogCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void UpdateTourLogs_ShouldClearAndPopulateTourLogs()
        {
            // Arrange
            var tourLogs = new List<TourLog>
            {
                new TourLog { Id = Guid.NewGuid(), Comment = "Log 1" },
                new TourLog { Id = Guid.NewGuid(), Comment = "Log 2" }
            };
            var testTour = new Tour
            {
                Id = Guid.NewGuid(),
                Name = "Test Tour",
                TourLogs = tourLogs
            };

            // Act
            _viewModel.UpdateTourLogs(testTour);

            // Assert
            Assert.That(_viewModel.TourLogs.Count, Is.EqualTo(2));
            Assert.That(_viewModel.TourLogs.First().Comment, Is.EqualTo("Log 1"));
            Assert.That(_viewModel.TourLogs.Last().Comment, Is.EqualTo("Log 2"));
        }
    }
}