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
    public class TourLogRepositoryTests
    {
        private TourPlannerDbContext _dbContext;
        private ITourRepository _repository;
        private ILoggerWrapper<TourRepository> _logger;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<TourPlannerDbContext>()
                .UseInMemoryDatabase(databaseName: "TourPlannerTestDb")
                .Options;

            // Initialize DbContext and Repository
            _dbContext = new TourPlannerDbContext(options);
            _logger = Substitute.For<ILoggerWrapper<TourRepository>>();
            _repository = new TourRepository(_dbContext, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the in-memory database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAllToursAsync_ShouldReturnEmpty_WhenNoToursExist()
        {
            // Act
            var tours = await _repository.GetAllToursAsync();
            // Assert
            Assert.That(tours, Is.Not.Null);
            Assert.That(tours, Is.Empty);
        }

        [Test]
        public async Task GetAllToursAsync_ShouldReturnAllTours_WhenToursExist()
        {
            // Arrange
            var tour1 = new TourEntity { Id = Guid.NewGuid(), Name = "Tour 1", From = "Start", To = "End" };
            var tour2 = new TourEntity { Id = Guid.NewGuid(), Name = "Tour 2", From = "Start", To = "End" };
            _dbContext.Tours.AddRange(tour1, tour2);
            await _dbContext.SaveChangesAsync();

            // Act
            var tours = await _repository.GetAllToursAsync();

            // Assert
            Assert.That(tours.Count(), Is.EqualTo(2));
            CollectionAssert.AreEquivalent(tours.Select(t => t.Id), tours.Select(t => t.Id));
        }

        [Test]
        public async Task AddTourAsync_ShouldAddTour_WhenTourIsValid()
        {
            // Arrange
            var newTour = new TourEntity
            {
                Id = Guid.NewGuid(),
                Name = "New Tour",
                From = "Start",
                To = "End",
                GeoJson = "{}",
                Directions = "Test directions",
                Distance = 100,
                EstimatedTime = 60,
                TransportType = "Car"
            };

            // Act
            await _repository.AddTourAsync(newTour);

            // Assert
            var dbTour = await _dbContext.Tours.FindAsync(newTour.Id);
            Assert.That(dbTour, Is.Not.Null);
            Assert.That(dbTour.Name, Is.EqualTo("New Tour"));
        }

        [Test]
        public async Task DeleteTourAsync_ShouldReturnTrue_WhenTourExists()
        {
            // Arrange
            var tour = new TourEntity { Id = Guid.NewGuid(), Name = "Tour to Delete", From = "Start", To = "End" };
            await _dbContext.Tours.AddAsync(tour);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteTourAsync(tour.Id);

            // Assert
            Assert.That(result, Is.True);
            var deletedTour = await _dbContext.Tours.FindAsync(tour.Id);
            Assert.That(deletedTour, Is.Null);
        }

        [Test]
        public async Task DeleteTourAsync_ShouldReturnFalse_WhenTourDoesNotExist()
        {
            // Arrange

            var nonExistentTourId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteTourAsync(nonExistentTourId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UpdateTourAsync_ShouldUpdateTour_WhenTourExists()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var tour = new TourEntity
            {
                Id = tourId,
                Name = "Original Tour",
                Description = "Original Description",
                From = "Vienna",
                To = "Salzburg",
                GeoJson = "{}",
                Directions = "Go straight",
                Distance = 100,
                EstimatedTime = 2.5,
                TransportType = "Car"
            };

            await _dbContext.Tours.AddAsync(tour);
            await _dbContext.SaveChangesAsync();

            var updatedTour = new TourEntity
            {
                Id = tourId,
                Name = "Updated Tour",
                Description = "Updated Description",
                From = "Graz",
                To = "Linz",
                GeoJson = "{}",
                Directions = "Go left",
                Distance = 150,
                EstimatedTime = 3.0,
                TransportType = "Bike"
            };

            // Act
            var result = await _repository.UpdateTourAsync(updatedTour);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated Tour"));
            Assert.That(result.Description, Is.EqualTo("Updated Description"));
            Assert.That(result.From, Is.EqualTo("Graz"));
            Assert.That(result.To, Is.EqualTo("Linz"));

            var dbTour = await _dbContext.Tours.FindAsync(tourId);
            Assert.That(dbTour!.Name, Is.EqualTo("Updated Tour"));
        }

        [Test]
        public void UpdateTourAsync_ShouldThrowTourNotFoundException_WhenTourDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var tour = new TourEntity
            {
                Id = nonExistentId,
                Name = "Non-existent Tour",
                Description = "Description",
                From = "Vienna",
                To = "Salzburg",
                GeoJson = "{}",
                Directions = "Go straight",
                Distance = 100,
                EstimatedTime = 2.5,
                TransportType = "Car"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourNotFoundException>(async () => await _repository.UpdateTourAsync(tour));
            Assert.That(ex.Message, Does.Contain(nonExistentId.ToString()));
        }

        [Test]
        public async Task TourNameExistsAsync_ShouldReturnFalse_WhenNameDoesNotExist()
        {
            // Arrange
            var tourName = "Unique Tour Name";

            // Act
            var exists = await _repository.TourNameExistsAsync(tourName);

            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task TourNameExistsAsync_ShouldReturnTrue_WhenNameExists()
        {
            // Arrange
            var tourName = "Existing Tour";
            var tour = new TourEntity
            {
                Id = Guid.NewGuid(),
                Name = tourName,
                From = "Start",
                To = "End"
            };
            await _dbContext.Tours.AddAsync(tour);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.TourNameExistsAsync(tourName);

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task TourNameExistsAsync_ShouldReturnFalse_WhenNameExistsButIdIsExcluded()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var tourName = "My Tour";
            var tour = new TourEntity
            {
                Id = tourId,
                Name = tourName,
                From = "Start",
                To = "End"
            };
            await _dbContext.Tours.AddAsync(tour);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.TourNameExistsAsync(tourName, tourId);

            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task TourNameExistsAsync_ShouldReturnTrue_WhenNameExistsForDifferentId()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var tourName = "Duplicate Tour";
            var tour = new TourEntity
            {
                Id = tourId,
                Name = tourName,
                From = "Start",
                To = "End"
            };
            await _dbContext.Tours.AddAsync(tour);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.TourNameExistsAsync(tourName, differentId);

            // Assert
            Assert.That(exists, Is.True);
        }
    }
}
