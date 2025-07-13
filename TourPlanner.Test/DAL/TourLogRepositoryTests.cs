using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TourPlanner.DAL.Context;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Exceptions;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.Repositories;
using TourPlanner.Logging;

namespace TourPlanner.Test.DAL
{
    [TestFixture]
    public class TourRepositoryTests
    {
        private TourPlannerDbContext _dbContext;
        private TourLogRepository _repository;
        private ILoggerWrapper<TourLogRepository> _logger;
        private Guid _tourId;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<TourPlannerDbContext>()
                .UseInMemoryDatabase(databaseName: $"TourPlannerTestDb_{Guid.NewGuid()}")
                .Options;

            // Initialize DbContext, Logger and Repository
            _dbContext = new TourPlannerDbContext(options);
            _logger = Substitute.For<ILoggerWrapper<TourLogRepository>>();
            _repository = new TourLogRepository(_dbContext, _logger);

            // Create a tour for our logs to reference
            _tourId = Guid.NewGuid();
            var tour = new TourEntity
            {
                Id = _tourId,
                Name = "Test Tour",
                Description = "Test Description",
                From = "Vienna",
                To = "Salzburg",
                GeoJson = "{}",
                Directions = "Go straight",
                Distance = 100,
                EstimatedTime = 2.0,
                TransportType = "Car",
                TourLogs = new List<TourLogEntity>()
            };

            _dbContext.Tours.Add(tour);
            _dbContext.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the in-memory database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetTourLogByIdAsync_ShouldReturnTourLog_WhenTourLogExists()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var date = DateTime.UtcNow;
            var tourLog = new TourLogEntity
            {
                Id = logId,
                Date = date,
                Comment = "Test comment",
                TotalDistance = 100.5,
                TotalTime = 2.5,
                Difficulty = 3.5,
                Rating = 4.0,
                TourId = _tourId
            };

            _dbContext.TourLogs.Add(tourLog);
            _dbContext.SaveChanges();

            // Act
            var result = await _repository.GetTourLogByIdAsync(logId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(logId));
            Assert.That(result.Comment, Is.EqualTo("Test comment"));
            Assert.That(result.TourId, Is.EqualTo(_tourId));
        }

        [Test]
        public async Task GetTourLogByIdAsync_ShouldReturnNull_WhenTourLogDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetTourLogByIdAsync(nonExistentId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddLogAsync_ShouldAddLogToDatabase_WhenLogIsValid()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var date = DateTime.UtcNow;
            var newLog = new TourLogEntity
            {
                Id = logId,
                Date = date,
                Comment = "New log comment",
                TotalDistance = 150.5,
                TotalTime = 3.5,
                Difficulty = 4.0,
                Rating = 4.5,
                TourId = _tourId
            };

            // Act
            var result = await _repository.AddLogAsync(newLog);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(logId));

            var dbLog = await _dbContext.TourLogs.FindAsync(logId);
            Assert.That(dbLog, Is.Not.Null);
            Assert.That(dbLog.Comment, Is.EqualTo("New log comment"));
            Assert.That(dbLog.TourId, Is.EqualTo(_tourId));
        }

        [Test]
        public async Task DeleteLogAsync_ShouldReturnTrue_WhenTourLogExists()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var tourLog = new TourLogEntity
            {
                Id = logId,
                Date = DateTime.UtcNow,
                Comment = "Log to delete",
                TotalDistance = 100.5,
                TotalTime = 2.5,
                Difficulty = 3.5,
                Rating = 4.0,
                TourId = _tourId
            };

            _dbContext.TourLogs.Add(tourLog);
            _dbContext.SaveChanges();

            // Act
            var result = await _repository.DeleteLogAsync(logId);

            // Assert
            Assert.That(result, Is.True);
            var deletedLog = await _dbContext.TourLogs.FindAsync(logId);
            Assert.That(deletedLog, Is.Null);
        }

        [Test]
        public async Task DeleteLogAsync_ShouldReturnFalse_WhenTourLogDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteLogAsync(nonExistentId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UpdateTourLogAsync_ShouldUpdateTourLog_WhenTourLogExists()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var originalDate = DateTime.UtcNow.AddDays(-1);
            var tourLog = new TourLogEntity
            {
                Id = logId,
                Date = originalDate,
                Comment = "Original comment",
                TotalDistance = 100.5,
                TotalTime = 2.5,
                Difficulty = 3.5,
                Rating = 4.0,
                TourId = _tourId
            };

            _dbContext.TourLogs.Add(tourLog);
            _dbContext.SaveChanges();

            var updatedDate = DateTime.UtcNow;
            var updatedLog = new TourLogEntity
            {
                Id = logId,
                Date = updatedDate,
                Comment = "Updated comment",
                TotalDistance = 120.0,
                TotalTime = 3.0,
                Difficulty = 4.0,
                Rating = 5.0,
                TourId = _tourId
            };

            // Act
            var result = await _repository.UpdateTourLogAsync(updatedLog);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Comment, Is.EqualTo("Updated comment"));
            Assert.That(result.TotalDistance, Is.EqualTo(120.0));
            Assert.That(result.Date, Is.EqualTo(updatedDate));

            var dbLog = await _dbContext.TourLogs.FindAsync(logId);
            Assert.That(dbLog!.Comment, Is.EqualTo("Updated comment"));
        }

        [Test]
        public void UpdateTourLogAsync_ShouldThrowTourLogNotFoundException_WhenTourLogDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updatedLog = new TourLogEntity
            {
                Id = nonExistentId,
                Date = DateTime.UtcNow,
                Comment = "Updated comment",
                TotalDistance = 120.0,
                TotalTime = 3.0,
                Difficulty = 4.0,
                Rating = 5.0,
                TourId = _tourId
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourLogNotFoundException>(
                async () => await _repository.UpdateTourLogAsync(updatedLog));

            Assert.That(ex.Message, Does.Contain(nonExistentId.ToString()));
        }
    }
}
