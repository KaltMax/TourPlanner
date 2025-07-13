using NSubstitute;
using TourPlanner.UI.ViewModels;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using System.Collections.ObjectModel;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class EditTourViewModelTests
    {
        private ITourDataService _mockTourService;
        private ISelectedTourService _mockSelectedTourService;
        private IUiCoordinator _mockUiCoordinator;
        private EditTourViewModel _viewModel;
        private Tour _testTour;

        [SetUp]
        public void Setup()
        {
            // Create mock dependencies
            _mockTourService = Substitute.For<ITourDataService>();
            _mockSelectedTourService = Substitute.For<ISelectedTourService>();
            _mockUiCoordinator = Substitute.For<IUiCoordinator>();

            // Create a test tour
            _testTour = new Tour
            {
                Id = Guid.NewGuid(),
                Name = "Original Tour",
                Description = "Original description",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            // Setup the Tours collection in the mock service
            var toursCollection = new ObservableCollection<Tour> { _testTour };
            _mockTourService.Tours.Returns(toursCollection);

            _mockSelectedTourService.SelectedTour.Returns(_testTour);

            // Initialize ViewModel with mocks
            _viewModel = new EditTourViewModel(_mockTourService, _mockSelectedTourService, _mockUiCoordinator);
        }

        [Test]
        public void Constructor_ShouldInitializeProperties_FromSelectedTour()
        {
            // Assert that ViewModel initializes correctly from SelectedTour
            Assert.That(_viewModel.TourName, Is.EqualTo("Original Tour"));
            Assert.That(_viewModel.Description, Is.EqualTo("Original description"));
            Assert.That(_viewModel.From, Is.EqualTo("Vienna"));
            Assert.That(_viewModel.To, Is.EqualTo("Salzburg"));
            Assert.That(_viewModel.TransportType, Is.EqualTo("Car"));
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenSelectedTourIsNull()
        {
            // Arrange: Mock selected tour as null
            Tour? nullTour = null;
            _mockSelectedTourService.SelectedTour.Returns(nullTour);

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new EditTourViewModel(_mockTourService, _mockSelectedTourService, _mockUiCoordinator));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("selectedTour"));
        }

        [Test]
        public void SaveCommand_ShouldBeDisabled_WhenNoChangesAreMade()
        {
            // Act: Check if SaveCommand is disabled
            bool canExecute = _viewModel.SaveCommand.CanExecute(null);

            // Assert: It should be disabled if no changes were made
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void SaveCommand_ShouldBeEnabled_WhenTourIsModified()
        {
            // Arrange: Modify the tour
            _viewModel.Description = "Updated description";

            // Act: Check if SaveCommand is enabled
            bool canExecute = _viewModel.SaveCommand.CanExecute(null);

            // Assert: It should be enabled when changes are made
            Assert.IsTrue(canExecute);
        }
    }
}