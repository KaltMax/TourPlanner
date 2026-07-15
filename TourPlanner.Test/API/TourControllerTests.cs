using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TourPlanner.API.Controllers;
using TourPlanner.API.DTOs;
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
            var result = (await _controller.Get()).ToList();

            // Assert: domain models are mapped to response DTOs, preserving identity.
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(r => r.Id), Is.EquivalentTo(expectedTours.Select(t => t.Id)));
            Assert.That(result.Select(r => r.Name), Is.EquivalentTo(expectedTours.Select(t => t.Name)));
            await _mockTourService.Received(1).GetAllToursAsync();
        }

        [Test]
        public async Task Get_ReturnsEmptyCollection_WhenNoToursExist()
        {
            // Arrange
            _mockTourService.GetAllToursAsync().Returns(new List<TourDomain>());

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
            var request = new TourRequest
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

            _mockTourService.AddTourAsync(Arg.Any<TourDomain>(), Arg.Any<CancellationToken>()).Returns(createdTour);

            // Act
            var result = await _controller.Post(request);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtActionResult!.Value, Is.TypeOf<TourResponse>());
            var response = (TourResponse)createdAtActionResult!.Value!;
            Assert.That(response.Id, Is.EqualTo(createdTour.Id));
            Assert.That(response.Name, Is.EqualTo("Test Tour"));
            // The client-supplied request is mapped to a domain model before reaching the service.
            await _mockTourService.Received(1).AddTourAsync(
                Arg.Is<TourDomain>(d => d != null && d.Name == "Test Tour" && d.From == "Vienna" && d.To == "Steyr"),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public void Post_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var request = new TourRequest { Name = "Test Tour" };
            var expectedException = new TourServiceException("Service error");
            _mockTourService.AddTourAsync(Arg.Any<TourDomain>(), Arg.Any<CancellationToken>()).Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceException>(() => _controller.Post(request));
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
            var request = new TourRequest
            {
                Name = "Updated Tour",
                Description = "Updated Description",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car"
            };

            var updatedTour = new TourDomain
            {
                Id = tourId,
                Name = "Updated Tour",
                Description = "Updated Description",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car"
            };

            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>(), Arg.Any<CancellationToken>()).Returns(updatedTour);

            // Act
            var result = await _controller.Put(tourId, request);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.TypeOf<TourResponse>());
            var response = (TourResponse)okResult.Value!;
            Assert.That(response.Id, Is.EqualTo(tourId));
            Assert.That(response.Name, Is.EqualTo("Updated Tour"));
            await _mockTourService.Received(1).UpdateTourAsync(tourId, Arg.Any<TourDomain>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Put_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var request = new TourRequest { Name = "Updated Tour" };
            var expectedException = new TourServiceNotFoundException("Tour not found");
            _mockTourService.UpdateTourAsync(tourId, Arg.Any<TourDomain>(), Arg.Any<CancellationToken>()).Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceNotFoundException>(() => _controller.Put(tourId, request));
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task ImportTours_ReturnsOk_WithImportedCount()
        {
            // Arrange
            var tours = new List<ImportTourRequest>
            {
                new() { Name = "Tour1", From = "Vienna", To = "Graz", TransportType = "Car" },
                new() { Name = "Tour2", From = "Linz", To = "Salzburg", TransportType = "Bike" }
            };
            _mockTourService.ImportToursAsync(Arg.Any<IEnumerable<TourDomain>>(), Arg.Any<CancellationToken>()).Returns(2);

            // Act
            var result = await _controller.ImportTours(tours);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            await _mockTourService.Received(1).ImportToursAsync(
                Arg.Is<IEnumerable<TourDomain>>(d => d != null && d.Count() == 2),
                Arg.Any<CancellationToken>());
        }
    }
}
