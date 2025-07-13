using NSubstitute;
using System.Collections.ObjectModel;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class TourListViewModelTests
    {
        private ITourDataService _mockDataService;
        private IUiCoordinator _mockUiCoordinator;
        private ISelectedTourService _mockSelectedTourService;
        private TourListViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create substitutes for dependencies
            _mockDataService = Substitute.For<ITourDataService>();
            _mockUiCoordinator = Substitute.For<IUiCoordinator>();
            _mockSelectedTourService = Substitute.For<ISelectedTourService>();

            // Setup default returns
            _mockDataService.Tours.Returns(new ObservableCollection<Tour>());

            // Create view model
            _viewModel = new TourListViewModel(_mockDataService, _mockUiCoordinator, _mockSelectedTourService);
        }

        [Test]
        public void SelectedTour_ShouldUpdateSelectedTourService_WhenSet()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour" };

            // Act
            _viewModel.SelectedTour = testTour;

            // Assert
            _mockSelectedTourService.Received(1).SelectedTour = testTour;
        }

        [Test]
        public void CanAddTour_ShouldReturnFalse_WhenEditingTour()
        {
            // Arrange
            _viewModel.IsEditingTour = true;

            // Act
            bool canExecute = _viewModel.AddTourCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.False);
        }

        [Test]
        public void CanEditTour_ShouldReturnTrue_WhenTourSelectedAndNoTourLogs()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour", TourLogs = new List<TourLog>() };
            _viewModel.SelectedTour = testTour;

            // Act
            bool canExecute = _viewModel.EditTourCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void CanRemoveTour_ShouldReturnFalse_WhenNoTourSelected()
        {
            // Arrange
            _viewModel.SelectedTour = null;

            // Act
            bool canExecute = _viewModel.RemoveTourCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.False);
        }

        [Test]
        public void AddTourCommand_ShouldOpenAddTourView_WhenNotAlreadyAdding()
        {
            // Arrange
            _viewModel.IsAddingTour = false;

            // Act
            _viewModel.AddTourCommand.Execute(null);

            // Assert
            _mockUiCoordinator.Received(1).OpenAddTourView();
            Assert.That(_viewModel.IsAddingTour, Is.True);
        }
    }
}