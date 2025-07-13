using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Exceptions;
using TourPlanner.BLL.Interfaces;
using TourPlanner.BLL.Services;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Exceptions;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Logging;

namespace TourPlanner.Test.BLL
{
    [TestFixture]
    public class TourLogServiceTests
    {
        private ITourLogRepository _tourLogRepository;
        private IMapper _mapper;
        private ILoggerWrapper<TourLogService> _logger;
        private ITourLogService _tourLogService;

        [SetUp]
        public void Setup()
        {
            // Setting up mock objects for dependencies
            _tourLogRepository = Substitute.For<ITourLogRepository>();
            _mapper = Substitute.For<IMapper>();
            _logger = Substitute.For<ILoggerWrapper<TourLogService>>();

            // Create an instance of the service to be tested
            _tourLogService = new TourLogService(_tourLogRepository, _mapper, _logger);
        }

        [Test]
        public async Task GetTourLogByIdAsync_ShouldReturnTourLog_WhenTourLogExists()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var tourLogEntity = new TourLogEntity
            {
                Id = logId,
                Date = DateTime.Now,
                Comment = "Test Comment",
                TotalDistance = 10.5,
                TotalTime = 2.5,
                Difficulty = 3.5,
                Rating = 4.0,
                TourId = Guid.NewGuid()
            };

            var expectedTourLog = new TourLogDomain
            {
                Id = logId,
                Date = tourLogEntity.Date,
                Comment = tourLogEntity.Comment,
                TotalDistance = tourLogEntity.TotalDistance,
                TotalTime = tourLogEntity.TotalTime,
                Difficulty = tourLogEntity.Difficulty,
                Rating = tourLogEntity.Rating,
                TourId = tourLogEntity.TourId
            };

            _tourLogRepository.GetTourLogByIdAsync(logId).Returns(tourLogEntity);
            _mapper.ToDomain(tourLogEntity).Returns(expectedTourLog);

            // Act
            var result = await _tourLogService.GetTourLogByIdAsync(logId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedTourLog));
            await _tourLogRepository.Received(1).GetTourLogByIdAsync(logId);
            _mapper.Received(1).ToDomain(tourLogEntity);
        }

        [Test]
        public async Task GetTourLogByIdAsync_ShouldReturnNull_WhenTourLogDoesNotExist()
        {
            // Arrange
            var logId = Guid.NewGuid();
            _tourLogRepository.GetTourLogByIdAsync(logId).Returns((TourLogEntity)null!);

            // Act
            var result = await _tourLogService.GetTourLogByIdAsync(logId);

            // Assert
            Assert.That(result, Is.Null);
            await _tourLogRepository.Received(1).GetTourLogByIdAsync(logId);
        }

        [Test]
        public void GetTourLogByIdAsync_ShouldThrowTourLogServiceException_WhenRepositoryThrowsException()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var repoException = new TourLogRepositoryException("Database error");
            _tourLogRepository.GetTourLogByIdAsync(logId).Throws(repoException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() => _tourLogService.GetTourLogByIdAsync(logId));
            Assert.That(ex.Message, Is.EqualTo($"Failed to retrieve tour log with ID {logId}"));
            Assert.That(ex.InnerException, Is.SameAs(repoException));
        }

        [Test]
        public void GetTourLogByIdAsync_ShouldThrowTourLogServiceException_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var unexpectedException = new Exception("Unexpected error");
            _tourLogRepository.GetTourLogByIdAsync(logId).Throws(unexpectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() => _tourLogService.GetTourLogByIdAsync(logId));
            Assert.That(ex.Message, Is.EqualTo($"Unexpected error retrieving tour log with ID {logId}"));
            Assert.That(ex.InnerException, Is.SameAs(unexpectedException));
        }

        [Test]
        public async Task AddLogAsync_ShouldAddAndReturnTourLog_WhenSuccessful()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var tourId = Guid.NewGuid();

            var newTourLog = new TourLogDomain
            {
                Id = logId,
                Date = DateTime.Now,
                Comment = "New Log",
                TotalDistance = 15.2,
                TotalTime = 3.0,
                Difficulty = 4.0,
                Rating = 5.0,
                TourId = tourId
            };

            var tourLogEntity = new TourLogEntity
            {
                Id = logId,
                Date = newTourLog.Date,
                Comment = newTourLog.Comment,
                TotalDistance = newTourLog.TotalDistance,
                TotalTime = newTourLog.TotalTime,
                Difficulty = newTourLog.Difficulty,
                Rating = newTourLog.Rating,
                TourId = tourId
            };

            var savedEntity = new TourLogEntity
            {
                Id = logId,
                Date = newTourLog.Date,
                Comment = newTourLog.Comment,
                TotalDistance = newTourLog.TotalDistance,
                TotalTime = newTourLog.TotalTime,
                Difficulty = newTourLog.Difficulty,
                Rating = newTourLog.Rating,
                TourId = tourId
            };

            var expectedResult = new TourLogDomain
            {
                Id = logId,
                Date = newTourLog.Date,
                Comment = newTourLog.Comment,
                TotalDistance = newTourLog.TotalDistance,
                TotalTime = newTourLog.TotalTime,
                Difficulty = newTourLog.Difficulty,
                Rating = newTourLog.Rating,
                TourId = tourId
            };

            _mapper.ToEntity(newTourLog).Returns(tourLogEntity);
            _tourLogRepository.AddLogAsync(tourLogEntity).Returns(savedEntity);
            _mapper.ToDomain(savedEntity).Returns(expectedResult);

            // Act
            var result = await _tourLogService.AddLogAsync(newTourLog);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedResult));
            _mapper.Received(1).ToEntity(newTourLog);
            await _tourLogRepository.Received(1).AddLogAsync(tourLogEntity);
            _mapper.Received(1).ToDomain(savedEntity);
        }

        [Test]
        public async Task AddLogAsync_ShouldGenerateId_WhenIdIsEmpty()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var newTourLog = new TourLogDomain
            {
                Id = Guid.Empty,
                Date = DateTime.Now,
                Comment = "New Log",
                TotalDistance = 15.2,
                TotalTime = 3.0,
                Difficulty = 4.0,
                Rating = 5.0,
                TourId = tourId
            };

            _mapper.ToEntity(Arg.Any<TourLogDomain>()).Returns(new TourLogEntity());
            _tourLogRepository.AddLogAsync(Arg.Any<TourLogEntity>()).Returns(new TourLogEntity());
            _mapper.ToDomain(Arg.Any<TourLogEntity>()).Returns(new TourLogDomain { Id = Guid.NewGuid() });

            // Act
            var result = await _tourLogService.AddLogAsync(newTourLog);

            // Assert
            Assert.That(newTourLog.Id, Is.Not.EqualTo(Guid.Empty));
            _mapper.Received(1).ToEntity(Arg.Is<TourLogDomain>(t => t.Id != Guid.Empty));
        }

        [Test]
        public void AddLogAsync_ShouldThrowTourLogServiceException_WhenRepositoryThrowsException()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var newTourLog = new TourLogDomain
            {
                Id = Guid.NewGuid(),
                TourId = tourId,
                Comment = "Test Log"
            };

            var repoException = new TourLogRepositoryException("Database error");

            _mapper.ToEntity(newTourLog).Returns(new TourLogEntity());
            _tourLogRepository.AddLogAsync(Arg.Any<TourLogEntity>()).Throws(repoException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() => _tourLogService.AddLogAsync(newTourLog));
            Assert.That(ex.Message, Is.EqualTo($"Failed to create tour log with ID {newTourLog.Id}"));
            Assert.That(ex.InnerException, Is.SameAs(repoException));
        }

        [Test]
        public void AddLogAsync_ShouldThrowTourLogServiceException_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var newTourLog = new TourLogDomain
            {
                Id = Guid.NewGuid(),
                TourId = tourId,
                Comment = "Test Log"
            };

            var unexpectedException = new Exception("Unexpected error");

            _mapper.ToEntity(newTourLog).Returns(new TourLogEntity());
            _tourLogRepository.AddLogAsync(Arg.Any<TourLogEntity>()).Throws(unexpectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() => _tourLogService.AddLogAsync(newTourLog));
            Assert.That(ex.Message, Is.EqualTo("Unexpected error creating tour log"));
            Assert.That(ex.InnerException, Is.SameAs(unexpectedException));
        }

        [Test]
        public async Task DeleteLogAsync_ShouldReturnTrue_WhenTourLogExists()
        {
            // Arrange
            var logId = Guid.NewGuid();
            _tourLogRepository.DeleteLogAsync(logId).Returns(true);

            // Act
            var result = await _tourLogService.DeleteLogAsync(logId);

            // Assert
            Assert.That(result, Is.True);
            await _tourLogRepository.Received(1).DeleteLogAsync(logId);
        }

        [Test]
        public async Task DeleteLogAsync_ShouldReturnFalse_WhenTourLogDoesNotExist()
        {
            // Arrange
            var logId = Guid.NewGuid();
            _tourLogRepository.DeleteLogAsync(logId).Returns(false);

            // Act
            var result = await _tourLogService.DeleteLogAsync(logId);

            // Assert
            Assert.That(result, Is.False);
            await _tourLogRepository.Received(1).DeleteLogAsync(logId);
        }

        [Test]
        public void DeleteLogAsync_ShouldThrowTourLogServiceException_WhenRepositoryThrowsException()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var repoException = new TourLogRepositoryException("Database error");
            _tourLogRepository.DeleteLogAsync(logId).Throws(repoException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() => _tourLogService.DeleteLogAsync(logId));
            Assert.That(ex.Message, Is.EqualTo($"Failed to delete tour log with ID {logId}"));
            Assert.That(ex.InnerException, Is.SameAs(repoException));
        }

        [Test]
        public void DeleteLogAsync_ShouldThrowTourLogServiceException_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var unexpectedException = new Exception("Unexpected error");
            _tourLogRepository.DeleteLogAsync(logId).Throws(unexpectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() => _tourLogService.DeleteLogAsync(logId));
            Assert.That(ex.Message, Is.EqualTo($"Unexpected error deleting tour log with ID {logId}"));
            Assert.That(ex.InnerException, Is.SameAs(unexpectedException));
        }

        [Test]
        public async Task UpdateTourLogAsync_ShouldUpdateAndReturnTourLog_WhenSuccessful()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var tourId = Guid.NewGuid();

            var updatedTourLog = new TourLogDomain
            {
                Id = Guid.Empty, // Will be overwritten
                Date = DateTime.Now,
                Comment = "Updated Log",
                TotalDistance = 20.5,
                TotalTime = 4.0,
                Difficulty = 3.0,
                Rating = 4.0,
                TourId = tourId
            };

            var tourLogEntity = new TourLogEntity
            {
                Id = logId,
                Date = updatedTourLog.Date,
                Comment = updatedTourLog.Comment,
                TotalDistance = updatedTourLog.TotalDistance,
                TotalTime = updatedTourLog.TotalTime,
                Difficulty = updatedTourLog.Difficulty,
                Rating = updatedTourLog.Rating,
                TourId = tourId
            };

            var savedEntity = new TourLogEntity
            {
                Id = logId,
                Date = updatedTourLog.Date,
                Comment = updatedTourLog.Comment,
                TotalDistance = updatedTourLog.TotalDistance,
                TotalTime = updatedTourLog.TotalTime,
                Difficulty = updatedTourLog.Difficulty,
                Rating = updatedTourLog.Rating,
                TourId = tourId
            };

            var expectedResult = new TourLogDomain
            {
                Id = logId,
                Date = updatedTourLog.Date,
                Comment = updatedTourLog.Comment,
                TotalDistance = updatedTourLog.TotalDistance,
                TotalTime = updatedTourLog.TotalTime,
                Difficulty = updatedTourLog.Difficulty,
                Rating = updatedTourLog.Rating,
                TourId = tourId
            };

            _mapper.ToEntity(Arg.Is<TourLogDomain>(t => t.Id == logId)).Returns(tourLogEntity);
            _tourLogRepository.UpdateTourLogAsync(tourLogEntity).Returns(savedEntity);
            _mapper.ToDomain(savedEntity).Returns(expectedResult);

            // Act
            var result = await _tourLogService.UpdateTourLogAsync(logId, updatedTourLog);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(updatedTourLog.Id, Is.EqualTo(logId));
            _mapper.Received(1).ToEntity(Arg.Is<TourLogDomain>(t => t.Id == logId));
            await _tourLogRepository.Received(1).UpdateTourLogAsync(tourLogEntity);
            _mapper.Received(1).ToDomain(savedEntity);
        }

        [Test]
        public void UpdateTourLogAsync_ShouldThrowTourLogServiceNotFoundException_WhenTourLogNotFound()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var updatedTourLog = new TourLogDomain
            {
                Comment = "Updated Log"
            };

            var notFoundException = new TourLogNotFoundException($"Tour log not found with ID {logId}");

            _mapper.ToEntity(Arg.Any<TourLogDomain>()).Returns(new TourLogEntity());
            _tourLogRepository.UpdateTourLogAsync(Arg.Any<TourLogEntity>()).Throws(notFoundException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceNotFoundException>(() =>
                _tourLogService.UpdateTourLogAsync(logId, updatedTourLog));

            Assert.That(ex.Message, Is.EqualTo($"TourLog with ID {logId} not found."));
            Assert.That(ex.InnerException, Is.SameAs(notFoundException));
        }

        [Test]
        public void UpdateTourLogAsync_ShouldThrowTourLogServiceException_WhenRepositoryThrowsException()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var updatedTourLog = new TourLogDomain
            {
                Comment = "Updated Log"
            };

            var repoException = new TourLogRepositoryException("Database error");

            _mapper.ToEntity(Arg.Any<TourLogDomain>()).Returns(new TourLogEntity());
            _tourLogRepository.UpdateTourLogAsync(Arg.Any<TourLogEntity>()).Throws(repoException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() =>
                _tourLogService.UpdateTourLogAsync(logId, updatedTourLog));

            Assert.That(ex.Message, Is.EqualTo($"Error occurred while updating the TourLog with ID {logId}."));
            Assert.That(ex.InnerException, Is.SameAs(repoException));
        }

        [Test]
        public void UpdateTourLogAsync_ShouldThrowTourLogServiceException_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var updatedTourLog = new TourLogDomain
            {
                Comment = "Updated Log"
            };

            var unexpectedException = new Exception("Unexpected error");

            _mapper.ToEntity(Arg.Any<TourLogDomain>()).Returns(new TourLogEntity());
            _tourLogRepository.UpdateTourLogAsync(Arg.Any<TourLogEntity>()).Throws(unexpectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogServiceException>(() =>
                _tourLogService.UpdateTourLogAsync(logId, updatedTourLog));

            Assert.That(ex.Message, Is.EqualTo($"Unexpected error updating tour log with ID {logId}"));
            Assert.That(ex.InnerException, Is.SameAs(unexpectedException));
        }
    }
}
