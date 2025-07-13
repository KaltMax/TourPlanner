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
        public async Task Get_ReturnsEmptyCollection_WhenServiceReturnsNUll()
        {
            // Arrange
            _mockTourService.GetAllToursAsync().Returns((IEnumerable < TourDomain>) null!);

            // Act
            var result = await _controller.Get();

            // Assert
            Assert.That(result, Is.Empty);
            await _mockTourService.Received(1).GetAllToursAsync();
        }

        [Test]
        public void Get_ThrowException_WhenServiceThrowsException()
        {
            // Arrange 
            var expectedException = new Exception("Test exception");
            _mockTourService.GetAllToursAsync().Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _controller.Get());
            Assert.That(ex, Is.EqualTo(expectedException));
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
        }

        [Test]
        public async Task Post_ReturnsBadRequest_WhenGeocodingExceptionOccurs()
        {
            // Arrange
            var newTour = new TourDomain { Name = "TestTour", From = "Invalid Location" };
            _mockTourService.AddTourAsync(Arg.Any<TourDomain>())
                .Throws(new GeocodingException("Geocoding failed"));

            // Act
            var result = await _controller.Post(newTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult!.Value, Is.EqualTo("Failed to geocode the provided locations."));
        }

        [Test]
        public async Task Post_ReturnsStatusCode502_WhenRoutingExceptionOccurs()
        {
            // Arrange
            var newTour = new TourDomain { Name = "Test Tour" };
            _mockTourService.AddTourAsync(Arg.Any<TourDomain>())
                .Throws(new RoutingException("Routing failed"));

            // Act
            var result = await _controller.Post(newTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result.Result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(502));
            Assert.That(statusCodeResult.Value, Is.EqualTo("Unable to retrieve routing information."));
        }

        [Test]
        public async Task Post_ReturnsConflict_WhenTourNameAlreadyExists()
        {
            // Arrange
            var newTour = new TourDomain { Name = "Existing Tour" };
            _mockTourService.AddTourAsync(Arg.Any<TourDomain>())
                .Throws(new TourNameAlreadyExistsException("Tour name already exists"));

            // Act
            var result = await _controller.Post(newTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
            var conflictResult = result.Result as ConflictObjectResult;
            Assert.That(conflictResult!.Value, Is.EqualTo("Tour name already exists"));
        }

        [Test]
        public async Task Post_ReturnsStatusCode500_WhenTourServiceExceptionOccurs()
        {
            // Arrange
            var newTour = new TourDomain { Name = "Test Tour" };
            _mockTourService.AddTourAsync(Arg.Any<TourDomain>())
                .Throws(new TourServiceException("Service error"));

            // Act
            var result = await _controller.Post(newTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result.Result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An internal service error occurred."));
        }

        [Test]
        public async Task Post_ReturnsInternalServerError_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var newTour = new TourDomain { Name = "Test Tour" };
            _mockTourService.AddTourAsync(Arg.Any<TourDomain>())
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = await _controller.Post(newTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result.Result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An internal error occurred."));
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
        public async Task Delete_ReturnsStatusCode500_WhenServiceExceptionOccurs()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            _mockTourService.DeleteTourAsync(tourId).Throws(new TourServiceException("Service error"));

            // Act
            var result = await _controller.Delete(tourId);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An internal service error occurred."));
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
        public async Task Delete_ReturnsStatusCode500_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            _mockTourService.DeleteTourAsync(tourId)
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = await _controller.Delete(tourId);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An internal error occurred."));
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
        public async Task Put_ReturnsBadRequest_WhenGeocodingExceptionOccurs()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain { Name = "Updated Tour", From = "Invalid Location" };
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>())
                .Throws(new GeocodingException("Invalid location"));

            // Act
            var result = await _controller.Put(tourId, updatedTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult!.Value, Is.EqualTo("Failed to geocode the provided locations."));
        }

        [Test]
        public async Task Put_ReturnsStatusCode502_WhenRoutingExceptionOccurs()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain { Name = "Updated Tour" };
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>())
                .Throws(new RoutingException("Routing failed"));

            // Act
            var result = await _controller.Put(tourId, updatedTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result.Result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(502));
            Assert.That(statusCodeResult.Value, Is.EqualTo("Unable to retrieve routing information."));
        }

        [Test]
        public async Task Put_ReturnsNotFound_WhenTourNotFound()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain { Name = "Updated Tour" };
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>())
                .Throws(new TourServiceNotFoundException("Tour not found"));

            // Act
            var result = await _controller.Put(tourId, updatedTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult!.Value, Is.EqualTo("Tour not found"));
        }

        [Test]
        public async Task Put_ReturnsConflict_WhenTourNameAlreadyExists()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain { Name = "Existing Tour" };
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>())
                .Throws(new TourNameAlreadyExistsException("Tour name already exists"));

            // Act
            var result = await _controller.Put(tourId, updatedTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
            var conflictResult = result.Result as ConflictObjectResult;
            Assert.That(conflictResult!.Value, Is.EqualTo("Tour name already exists"));
        }

        [Test]
        public async Task Put_ReturnsStatusCode500_WhenTourServiceExceptionOccurs()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain { Name = "Updated Tour" };
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>())
                .Throws(new TourServiceException("Service error"));

            // Act
            var result = await _controller.Put(tourId, updatedTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result.Result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An internal service error occurred."));
        }

        [Test]
        public async Task Put_ReturnsStatusCode500_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain { Name = "Updated Tour" };
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>())
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = await _controller.Put(tourId, updatedTour);

            // Assert
            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var statusCodeResult = result.Result as ObjectResult;
            Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An internal error occurred."));
        }
    }
}
