using NSubstitute;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class TourInfoViewModelTests
    {
        private ISelectedTourService _mockSelectedTourService;
        private IMapService _mockMapService;
        private TourInfoViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create substitutes for dependencies
            _mockSelectedTourService = Substitute.For<ISelectedTourService>();
            _mockMapService = Substitute.For<IMapService>();

            // Setup default returns
            _mockMapService.IsInitialized.Returns(true);

            // Create view model
            _viewModel = new TourInfoViewModel(_mockSelectedTourService, _mockMapService);
        }

        [Test]
        public void Constructor_ShouldInitializeSelectedTour_FromSelectedTourService()
        {
            // Arrange
            var testTour = new Tour { Id = Guid.NewGuid(), Name = "Test Tour" };
            _mockSelectedTourService.SelectedTour.Returns(testTour);

            // Act
            var viewModel = new TourInfoViewModel(_mockSelectedTourService, _mockMapService);

            // Assert
            Assert.That(viewModel.SelectedTour, Is.EqualTo(testTour));
        }

        [Test]
        public void IsMapAvailable_ShouldReturnMapServiceInitializationStatus()
        {
            // Arrange
            _mockMapService.IsInitialized.Returns(false);

            // Act
            bool isAvailable = _viewModel.IsMapAvailable;

            // Assert
            Assert.That(isAvailable, Is.False);
        }

        [Test]
        public async Task OnTourChanged_ShouldLoadGeoJsonMap_WhenTourHasGeoJson()
        {
            // Arrange
            var testTour = new Tour
            {
                Id = Guid.NewGuid(),
                Name = "Test Tour",
                GeoJson = "{\"type\":\"FeatureCollection\"}"
            };
            _mockMapService.IsInitialized.Returns(true);

            // Act
            _viewModel.SelectedTour = testTour;
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            await _mockMapService.Received(1).LoadGeoJsonMapAsync("{\"type\":\"FeatureCollection\"}");
        }
    }
}