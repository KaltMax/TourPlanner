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
    public class TourLogControllerTests
    {
        private TourLogController _controller;
        private ITourLogService _mockTourLogService;
        private ILoggerWrapper<TourLogController> _mockLogger;

        [SetUp]
        public void Setup()
        {
            // Create mock dependencies
            _mockTourLogService = Substitute.For<ITourLogService>();
            _mockLogger = Substitute.For<ILoggerWrapper<TourLogController>>();

            // Create the controller with mock dependencies
            _controller = new TourLogController(_mockTourLogService, _mockLogger);
        }

        [Test]
        public async Task GetById_ReturnsOk_WhenTourLogExists()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var expectedLog = new TourLogDomain
            {
                Id = logId,
                TourId = Guid.NewGuid(),
                Date = DateTime.Now,
                Comment = "Test Comment",
                Difficulty = 3.0,
                TotalDistance = 15.2,
                TotalTime = 2.5,
                Rating = 4.0
            };

            _mockTourLogService.GetTourLogByIdAsync(logId).Returns(expectedLog);

            // Act
            var result = await _controller.GetById(logId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.SameAs(expectedLog));
            await _mockTourLogService.Received(1).GetTourLogByIdAsync(logId);
        }

        [Test]
        public async Task GetById_ReturnsNotFound_WhenTourLogDoesNotExist()
        {
            // Arrange
            var logId = Guid.NewGuid();
            _mockTourLogService.GetTourLogByIdAsync(logId).Returns((TourLogDomain)null!);

            // Act
            var result = await _controller.GetById(logId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
            await _mockTourLogService.Received(1).GetTourLogByIdAsync(logId);
        }

        [Test]
        public async Task GetById_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var expectedException = new TourLogServiceException("Service error");
            _mockTourLogService.GetTourLogByIdAsync(logId).Throws(expectedException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourLogServiceException>(() => _controller.GetById(logId));
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task Post_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var newTourLog = new TourLogDomain
            {
                TourId = tourId,
                Date = DateTime.Now,
                Comment = "New log entry",
                Difficulty = 3.0,
                TotalDistance = 10.5,
                TotalTime = 1.5,
                Rating = 4.0
            };

            var createdTourLog = new TourLogDomain
            {
                Id = Guid.NewGuid(),
                TourId = tourId,
                Date = newTourLog.Date,
                Comment = newTourLog.Comment,
                Difficulty = newTourLog.Difficulty,
                TotalDistance = newTourLog.TotalDistance,
                TotalTime = newTourLog.TotalTime,
                Rating = newTourLog.Rating
            };

            _mockTourLogService.AddLogAsync(Arg.Any<TourLogDomain>()).Returns(createdTourLog);

            // Act
            var result = await _controller.Post(newTourLog);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());

            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtActionResult!.ActionName, Is.EqualTo(nameof(TourLogController.GetById)));
            Assert.That(createdAtActionResult.RouteValues!["id"], Is.EqualTo(createdTourLog.Id));
            Assert.That(createdAtActionResult.Value, Is.EqualTo(createdTourLog));

            await _mockTourLogService.Received(1).AddLogAsync(Arg.Is<TourLogDomain>(t =>
                t != null &&
                t.TourId == newTourLog.TourId &&
                t.Comment == newTourLog.Comment));
        }

        [Test]
        public async Task Post_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var newTourLog = new TourLogDomain { TourId = Guid.NewGuid(), Comment = "Test log" };
            var expectedException = new TourLogServiceException("Service error");
            _mockTourLogService.AddLogAsync(Arg.Any<TourLogDomain>()).Throws(expectedException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourLogServiceException>(() => _controller.Post(newTourLog));
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var logId = Guid.NewGuid();
            _mockTourLogService.DeleteLogAsync(logId).Returns(true);

            // Act
            var result = await _controller.Delete(logId);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
            await _mockTourLogService.Received(1).DeleteLogAsync(logId);
        }

        [Test]
        public async Task Delete_ReturnsNotFound_WhenTourLogNotFound()
        {
            // Arrange
            var logId = Guid.NewGuid();
            _mockTourLogService.DeleteLogAsync(logId).Returns(false);

            // Act
            var result = await _controller.Delete(logId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
            await _mockTourLogService.Received(1).DeleteLogAsync(logId);
        }

        [Test]
        public async Task Delete_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var expectedException = new TourLogServiceException("Service error");
            _mockTourLogService.DeleteLogAsync(logId).Throws(expectedException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourLogServiceException>(() => _controller.Delete(logId));
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task Put_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var updatedTourLog = new TourLogDomain
            {
                Id = logId,
                TourId = Guid.NewGuid(),
                Date = DateTime.Now,
                Comment = "Updated comment",
                Difficulty = 4.0,
                TotalDistance = 12.0,
                TotalTime = 2.0,
                Rating = 5.0
            };

            _mockTourLogService.UpdateTourLogAsync(logId, Arg.Any<TourLogDomain>()).Returns(updatedTourLog);

            // Act
            var result = await _controller.Put(logId, updatedTourLog);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.SameAs(updatedTourLog));
            await _mockTourLogService.Received(1).UpdateTourLogAsync(logId, Arg.Any<TourLogDomain>());
        }

        [Test]
        public async Task Put_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var updatedTourLog = new TourLogDomain { Id = logId, Comment = "Updated comment" };
            var expectedException = new TourLogServiceNotFoundException("Tour log not found");
            _mockTourLogService.UpdateTourLogAsync(logId, Arg.Any<TourLogDomain>()).Throws(expectedException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourLogServiceNotFoundException>(() => _controller.Put(logId, updatedTourLog));
            Assert.That(ex, Is.SameAs(expectedException));
        }
    }
}
