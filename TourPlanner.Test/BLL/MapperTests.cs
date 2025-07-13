using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Mappers;
using TourPlanner.BLL.Interfaces;
using TourPlanner.DAL.Entities;

namespace TourPlanner.Test.BLL
{
    [TestFixture]
    public class MapperTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = new Mapper();
        }

        [Test]
        public void ToDomain_ShouldMapTourEntityToDomainModel_WithAllProperties()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var tourEntity = new TourEntity
            {
                Id = tourId,
                Name = "Test Tour",
                Description = "A test tour",
                From = "Vienna",
                To = "Salzburg",
                GeoJson = "GeoJsonString",
                Directions = "Go straight",
                Distance = 250.5,
                EstimatedTime = 3.5,
                TransportType = "Car",
                TourLogs = new List<TourLogEntity>
                {
                    new TourLogEntity
                    {
                        Id = Guid.NewGuid(),
                        Date = new DateTime(),
                        Comment = "Great tour",
                        TotalDistance = 255.2,
                        TotalTime = 4.0,
                        Difficulty = 3.0,
                        Rating = 4.0,
                        TourId = tourId
                    }
                }
            };

            // Act
            var tourDomain = _mapper.ToDomain(tourEntity);

            // Assert
            Assert.That(tourDomain, Is.Not.Null);
            Assert.That(tourDomain.Id, Is.EqualTo(tourEntity.Id));
            Assert.That(tourDomain.Name, Is.EqualTo(tourEntity.Name));
            Assert.That(tourDomain.Description, Is.EqualTo(tourEntity.Description));
            Assert.That(tourDomain.From, Is.EqualTo(tourEntity.From));
            Assert.That(tourDomain.To, Is.EqualTo(tourEntity.To));
            Assert.That(tourDomain.GeoJson, Is.EqualTo(tourEntity.GeoJson));
            Assert.That(tourDomain.Directions, Is.EqualTo(tourEntity.Directions));
            Assert.That(tourDomain.Distance, Is.EqualTo(tourEntity.Distance));
            Assert.That(tourDomain.EstimatedTime, Is.EqualTo(tourEntity.EstimatedTime));
            Assert.That(tourDomain.TransportType, Is.EqualTo(tourEntity.TransportType));

            // Check tour logs
            Assert.That(tourDomain.TourLogs, Is.Not.Null);
            Assert.That(tourDomain.TourLogs.Count, Is.EqualTo(1));
            Assert.That(tourDomain.TourLogs[0].Id, Is.EqualTo(tourEntity.TourLogs.First().Id));
            Assert.That(tourDomain.TourLogs[0].Comment, Is.EqualTo(tourEntity.TourLogs.First().Comment));
            Assert.That(tourDomain.TourLogs[0].TourId, Is.EqualTo(tourId));
        }

        [Test]
        public void ToEntity_ShouldMapTourDomainToEntity_WithAllProperties()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var tourDomain = new TourDomain
            {
                Id = tourId,
                Name = "Test Tour",
                Description = "A test tour",
                From = "Vienna",
                To = "Salzburg",
                GeoJson = "GeoJsonString",
                Directions = "Go straight",
                Distance = 250.5,
                EstimatedTime = 3.5,
                TransportType = "Car",
                TourLogs = new List<TourLogDomain>
                {
                    new TourLogDomain
                    {
                        Id = Guid.NewGuid(),
                        Date = new DateTime(),
                        Comment = "Great tour",
                        TotalDistance = 255.2,
                        TotalTime = 4.0,
                        Difficulty = 3.0,
                        Rating = 4.0,
                        TourId = tourId
                    }
                }
            };

            // Act
            var tourEntity = _mapper.ToEntity(tourDomain);

            // Assert
            Assert.That(tourEntity, Is.Not.Null);
            Assert.That(tourEntity.Id, Is.EqualTo(tourDomain.Id));
            Assert.That(tourEntity.Name, Is.EqualTo(tourDomain.Name));
            Assert.That(tourEntity.Description, Is.EqualTo(tourDomain.Description));
            Assert.That(tourEntity.From, Is.EqualTo(tourDomain.From));
            Assert.That(tourEntity.To, Is.EqualTo(tourDomain.To));
            Assert.That(tourEntity.GeoJson, Is.EqualTo(tourDomain.GeoJson));
            Assert.That(tourEntity.Directions, Is.EqualTo(tourDomain.Directions));
            Assert.That(tourEntity.Distance, Is.EqualTo(tourDomain.Distance));
            Assert.That(tourEntity.EstimatedTime, Is.EqualTo(tourDomain.EstimatedTime));
            Assert.That(tourEntity.TransportType, Is.EqualTo(tourDomain.TransportType));

            Assert.That(tourEntity.TourLogs, Is.Not.Null);
            Assert.That(tourEntity.TourLogs.Count, Is.EqualTo(1));
        }

        [Test]
        public void ToDomain_ShouldMapTourLogEntityToDomain_WithAllProperties()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var logId = Guid.NewGuid();
            var logEntity = new TourLogEntity
            {
                Id = logId,
                Date = new DateTime(),
                Comment = "Great tour",
                TotalDistance = 255.2,
                TotalTime = 4.0,
                Difficulty = 3.0,
                Rating = 4.0,
                TourId = tourId
            };

            // Act
            var logDomain = _mapper.ToDomain(logEntity);

            // Assert
            Assert.That(logDomain, Is.Not.Null);
            Assert.That(logDomain.Id, Is.EqualTo(logEntity.Id));
            Assert.That(logDomain.Date, Is.EqualTo(logEntity.Date));
            Assert.That(logDomain.Comment, Is.EqualTo(logEntity.Comment));
            Assert.That(logDomain.TotalDistance, Is.EqualTo(logEntity.TotalDistance));
            Assert.That(logDomain.TotalTime, Is.EqualTo(logEntity.TotalTime));
            Assert.That(logDomain.Difficulty, Is.EqualTo(logEntity.Difficulty));
            Assert.That(logDomain.Rating, Is.EqualTo(logEntity.Rating));
            Assert.That(logDomain.TourId, Is.EqualTo(logEntity.TourId));
        }

        [Test]
        public void ToEntity_ShouldMapTourLogDomainToEntity_WithAllProperties()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var logId = Guid.NewGuid();
            var logDate = new DateTime();
            var logDomain = new TourLogDomain
            {
                Id = logId,
                Date = logDate,
                Comment = "Great tour",
                TotalDistance = 255.2,
                TotalTime = 4.0,
                Difficulty = 3.0,
                Rating = 4.0,
                TourId = tourId
            };

            // Act
            var logEntity = _mapper.ToEntity(logDomain);

            // Assert
            Assert.That(logEntity, Is.Not.Null);
            Assert.That(logEntity.Id, Is.EqualTo(logDomain.Id));
            Assert.That(logEntity.Date, Is.EqualTo(logDate.ToUniversalTime()));
            Assert.That(logEntity.Comment, Is.EqualTo(logDomain.Comment));
            Assert.That(logEntity.TotalDistance, Is.EqualTo(logDomain.TotalDistance));
            Assert.That(logEntity.TotalTime, Is.EqualTo(logDomain.TotalTime));
            Assert.That(logEntity.Difficulty, Is.EqualTo(logDomain.Difficulty));
            Assert.That(logEntity.Rating, Is.EqualTo(logDomain.Rating));
            Assert.That(logEntity.TourId, Is.EqualTo(logDomain.TourId));
        }

        [Test]
        public void ToEntity_ShouldConvertLocalDateTimeToUtc_ForTourLog()
        {
            // Arrange
            var localDate = new DateTime();
            var logDomain = new TourLogDomain
            {
                Id = Guid.NewGuid(),
                Date = localDate,
                Comment = "Test log",
                TourId = Guid.NewGuid()
            };

            // Act
            var logEntity = _mapper.ToEntity(logDomain);

            // Assert
            Assert.That(logEntity.Date.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(logEntity.Date, Is.EqualTo(localDate.ToUniversalTime()));
        }
    }
}
