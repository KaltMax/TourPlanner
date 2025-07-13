using NSubstitute;
using System.Collections.ObjectModel;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class EditTourLogViewModelTests
    {
        private ITourDataService _mockTourDataService;
        private ISelectedTourService _mockSelectedTourService;
        private IUiCoordinator _mockUiCoordinator;
        private Tour _testTour;
        private TourLog _testTourLog;
        private EditTourLogViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create substitutes for dependencies
            _mockTourDataService = Substitute.For<ITourDataService>();
            _mockSelectedTourService = Substitute.For<ISelectedTourService>();
            _mockUiCoordinator = Substitute.For<IUiCoordinator>();

            // Create test data
            _testTour = new Tour
            {
                Id = Guid.NewGuid(),
                Name = "Test Tour",
                TourLogs = new List<TourLog>()
            };

            _testTourLog = new TourLog
            {
                Id = Guid.NewGuid(),
                Comment = "Original Comment",
                TotalDistance = 10.0,
                TotalTime = 2.0,
                Difficulty = 3.0,
                Rating = 4.0,
                TourId = _testTour.Id
            };

            _testTour.TourLogs.Add(_testTourLog);

            // Setup mock returns
            _mockSelectedTourService.SelectedTour.Returns(_testTour);
            _mockSelectedTourService.SelectedTourLog.Returns(_testTourLog);
            _mockTourDataService.Tours.Returns(new ObservableCollection<Tour> { _testTour });

            // Create view model
            _viewModel = new EditTourLogViewModel(_mockTourDataService, _mockSelectedTourService, _mockUiCoordinator);
        }

        [Test]
        public void Constructor_ShouldInitializeProperties_FromOriginalTourLog()
        {
            // Assert
            Assert.That(_viewModel.Comment, Is.EqualTo("Original Comment"));
            Assert.That(_viewModel.TotalDistance, Is.EqualTo(10.0));
            Assert.That(_viewModel.TotalTime, Is.EqualTo(2.0));
            Assert.That(_viewModel.Difficulty, Is.EqualTo(3.0));
            Assert.That(_viewModel.Rating, Is.EqualTo(4.0));
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenSelectedTourLogIsNull()
        {
            // Arrange
            _mockSelectedTourService.SelectedTourLog.Returns((TourLog?)null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new EditTourLogViewModel(_mockTourDataService, _mockSelectedTourService, _mockUiCoordinator));

            Assert.That(ex.Message, Is.EqualTo("No tour or tour log selected for editing."));
        }

        [Test]
        public void CanSave_ShouldReturnFalse_WhenValidationFails()
        {
            // Test multiple validation scenarios
            _viewModel.Comment = "";
            Assert.That(_viewModel.SaveCommand.CanExecute(null), Is.False);

            _viewModel.Comment = "Valid Comment";
            _viewModel.TotalDistance = 0;
            Assert.That(_viewModel.SaveCommand.CanExecute(null), Is.False);

            _viewModel.TotalDistance = 10.0;
            _viewModel.TotalTime = 0;
            Assert.That(_viewModel.SaveCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void CanSave_ShouldReturnTrue_WhenValidChangesAreMade()
        {
            // Act
            _viewModel.Comment = "Modified Comment";

            // Assert
            Assert.That(_viewModel.SaveCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task SaveCommand_ShouldUpdateTourLogAndCloseView()
        {
            // Arrange
            _viewModel.Comment = "Modified Comment";
            _viewModel.TotalDistance = 15.0;

            // Act
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            await _mockTourDataService.Received(1).UpdateTourLogAsync(_testTourLog);
            _mockUiCoordinator.Received(1).CloseEditTourLogView();
            Assert.That(_testTourLog.Comment, Is.EqualTo("Modified Comment"));
            Assert.That(_testTourLog.TotalDistance, Is.EqualTo(15.0));
        }

        [Test]
        public void CancelCommand_ShouldCloseEditTourLogView()
        {
            // Act
            _viewModel.CancelCommand.Execute(null);

            // Assert
            _mockUiCoordinator.Received(1).CloseEditTourLogView();
        }
    }
}