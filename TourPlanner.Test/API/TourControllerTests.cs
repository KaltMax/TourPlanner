using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TourPlanner.API.Controllers;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Exceptions;
using TourPlanner.BLL.Interfaces;
using TourPlanner.Logging;

namespace TourPlanner.Test.API
{
    [TestFixture]
    public class TourControllerTests
    {
        private TourController _controller;
        private ITourService _mockTourService;
        private ILoggerWrapper<TourController> _mockLogger;

        [SetUp]
        public void Setup()
        {
            // Create mock dependencies
            _mockTourService = Substitute.For<ITourService>();
            _mockLogger = Substitute.For<ILoggerWrapper<TourController>>();

            // Create the controller with mock dependencies
            _controller = new TourController(_mockTourService, _mockLogger);
        }

        [Test]
        public async Task Get_ReturnsAllTours_WhenSuccessful()
        {
            // Arrange
            var expectedTours = new List<TourDomain>
            {
                new TourDomain { Id = Guid.NewGuid(), Name = "Tour1" },
                new TourDomain { Id = Guid.NewGuid(), Name = "Tour2" }
            };

            _mockTourService.GetAllToursAsync().Returns(expectedTours);

            // Act
            var result = await _controller.Get();

            // Assert
            Assert.That(result, Is.EqualTo(expectedTours));
            await _mockTourService.Received(1).GetAllToursAsync();
        }

        [Test]
        public async Task Get_ReturnsEmptyCollection_WhenServiceReturnsNull()
        {
            // Arrange
            _mockTourService.GetAllToursAsync().Returns((IEnumerable<TourDomain>)null!);

            // Act
            var result = await _controller.Get();

            // Assert
            Assert.That(result, Is.Empty);
            await _mockTourService.Received(1).GetAllToursAsync();
        }

        [Test]
        public void Get_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var expectedException = new Exception("Test exception");
            _mockTourService.GetAllToursAsync().Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _controller.Get());
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task Post_ReturnsCreatedAction_WhenSuccessful()
        {
            // Arrange
            var newTour = new TourDomain
            {
                Name = "Test Tour",
                Description = "Test Description",
                From = "Vienna",
                To = "Steyr",
                TransportType = "Car"
            };

            var createdTour = new TourDomain
            {
                Id = Guid.NewGuid(),
                Name = "Test Tour",
                Description = "Test Description",
                From = "Vienna",
                To = "Steyr",
                TransportType = "Car"
            };

            _mockTourService.AddTourAsync(newTour).Returns(createdTour);

            // Act
            var result = await _controller.Post(newTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtActionResult!.Value, Is.SameAs(createdTour));
        }

        [Test]
        public void Post_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var newTour = new TourDomain { Name = "Test Tour" };
            var expectedException = new TourServiceException("Service error");
            _mockTourService.AddTourAsync(Arg.Any<TourDomain>()).Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceException>(() => _controller.Post(newTour));
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            _mockTourService.DeleteTourAsync(tourId).Returns(true);

            // Act
            var result = await _controller.Delete(tourId);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
            await _mockTourService.Received(1).DeleteTourAsync(tourId);
        }

        [Test]
        public async Task Delete_ReturnsNotFound_WhenTourNotFound()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            _mockTourService.DeleteTourAsync(tourId).Returns(false);

            // Act
            var result = await _controller.Delete(tourId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
            await _mockTourService.Received(1).DeleteTourAsync(tourId);
        }

        [Test]
        public void Delete_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var expectedException = new TourServiceException("Service error");
            _mockTourService.DeleteTourAsync(tourId).Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceException>(() => _controller.Delete(tourId));
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task Put_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Id = tourId,
                Name = "Updated Tour",
                Description = "Updated Description"
            };

            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>()).Returns(updatedTour);

            // Act
            var result = await _controller.Put(tourId, updatedTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.SameAs(updatedTour));
            await _mockTourService.Received(1).UpdateTourAsync(tourId, Arg.Any<TourDomain>());
        }

        [Test]
        public void Put_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain { Name = "Updated Tour" };
            var expectedException = new TourServiceNotFoundException("Tour not found");
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>()).Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceNotFoundException>(() => _controller.Put(tourId, updatedTour));
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task ImportTours_ReturnsOk_WithImportedCount()
        {
            // Arrange
            var tours = new List<TourDomain>
            {
                new TourDomain { Name = "Tour1" },
                new TourDomain { Name = "Tour2" }
            };
            _mockTourService.ImportToursAsync(tours).Returns(2);

            // Act
            var result = await _controller.ImportTours(tours);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            await _mockTourService.Received(1).ImportToursAsync(tours);
        }
    }
}
