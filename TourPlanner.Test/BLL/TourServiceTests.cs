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
    public class TourServiceTests
    {
        private IRouteService _routeService;
        private ITourRepository _tourRepository;
        private IMapper _mapper;
        private ILoggerWrapper<TourService> _logger;
        private ITourService _tourService;

        [SetUp]
        public void Setup()
        {
            // Initialize substitutes for dependencies
            _routeService = Substitute.For<IRouteService>();
            _tourRepository = Substitute.For<ITourRepository>();
            _mapper = Substitute.For<IMapper>();
            _logger = Substitute.For<ILoggerWrapper<TourService>>();

            // Create an instance of TourService with substituted dependencies
            _tourService = new TourService(_routeService, _tourRepository, _mapper, _logger);
        }

        [Test]
        public async Task GetAllToursAsync_ShouldReturnMappedTours_WhenRepositoryReturnsEntities()
        {
            // Arrange
            var tourEntities = new List<TourEntity>
            {
                new TourEntity { Id = Guid.NewGuid(), Name = "Tour 1" },
                new TourEntity { Id = Guid.NewGuid(), Name = "Tour 2" }
            };

            var tourDomains = new List<TourDomain>
            {
                new TourDomain { Id = tourEntities[0].Id, Name = "Tour 1" },
                new TourDomain { Id = tourEntities[1].Id, Name = "Tour 2" }
            };

            _tourRepository.GetAllToursAsync().Returns(tourEntities);

            _mapper.ToDomain(tourEntities[0]).Returns(tourDomains[0]);
            _mapper.ToDomain(tourEntities[1]).Returns(tourDomains[1]);

            // Act
            var result = await _tourService.GetAllToursAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result, Is.EquivalentTo(tourDomains));
            await _tourRepository.Received(1).GetAllToursAsync();
        }

        [Test]
        public void GetAllToursAsync_ShouldPropagateException_WhenRepositoryThrows()
        {
            // Arrange
            var expectedException = new Exception("Repository error");
            _tourRepository.GetAllToursAsync().Throws(expectedException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _tourService.GetAllToursAsync());
            Assert.That(ex, Is.SameAs(expectedException));
        }

        [Test]
        public async Task AddTourAsync_ShouldAddAndReturnTour_WhenEverythingIsCorrect()
        {
            // Arrange
            var newTourId = Guid.NewGuid();
            var newTour = new TourDomain
            {
                Id = newTourId,
                Name = "New Tour",
                Description = "Tour Description",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            var routeInfo = new RouteInfo
            {
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            var tourEntity = new TourEntity
            {
                Id = newTourId,
                Name = "New Tour",
                Description = "Tour Description",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car",
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            var savedTourEntity = new TourEntity
            {
                Id = newTourId,
                Name = "New Tour",
                Description = "Tour Description",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car",
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            var expectedTourDomain = new TourDomain
            {
                Id = newTourId,
                Name = "New Tour",
                Description = "Tour Description",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car",
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            _routeService.GetRouteInfoAsync(newTour.From, newTour.To, newTour.TransportType).Returns(routeInfo);

            _mapper.ToEntity(Arg.Is<TourDomain>(d =>
                d.Id == newTourId && d.Name == "New Tour")).Returns(tourEntity);

            _tourRepository.AddTourAsync(tourEntity).Returns(savedTourEntity);

            _mapper.ToDomain(savedTourEntity).Returns(expectedTourDomain);

            // Act
            var result = await _tourService.AddTourAsync(newTour);

            // Assert
            Assert.That(result, Is.EqualTo(expectedTourDomain));
            await _routeService.Received(1).GetRouteInfoAsync(newTour.From, newTour.To, newTour.TransportType);
            await _tourRepository.Received(1).AddTourAsync(tourEntity);
        }

        [Test]
        public async Task AddTourAsync_ShouldGenerateId_WhenTourIdIsEmpty()
        {
            // Arrange
            var newTour = new TourDomain
            {
                Id = Guid.Empty,
                Name = "New Tour",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            var routeInfo = new RouteInfo
            {
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            _routeService.GetRouteInfoAsync(newTour.From, newTour.To, newTour.TransportType).Returns(routeInfo);

            _mapper.ToEntity(Arg.Any<TourDomain>()).Returns(new TourEntity());

            _tourRepository.AddTourAsync(Arg.Any<TourEntity>()).Returns(new TourEntity());

            _mapper.ToDomain(Arg.Any<TourEntity>()).Returns(new TourDomain { Id = Guid.NewGuid() });

            // Act
            var result = await _tourService.AddTourAsync(newTour);

            // Assert
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void AddTourAsync_ShouldThrowTourNameAlreadyExistsException_WhenNameExists()
        {
            // Arrange
            var newTour = new TourDomain
            {
                Id = Guid.NewGuid(),
                Name = "Existing Tour Name",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            // Setup repository to return that name already exists
            _tourRepository.TourNameExistsAsync(newTour.Name, newTour.Id).Returns(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourNameAlreadyExistsException>(() => _tourService.AddTourAsync(newTour));

            Assert.That(ex.Message, Is.EqualTo($"A tour with the name '{newTour.Name}' already exists."));

            // Verify the repository was called with correct parameters
            _tourRepository.Received(1).TourNameExistsAsync(newTour.Name, newTour.Id);

            // Verify that routing service was never called (early return)
            _routeService.DidNotReceive().GetRouteInfoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void AddTourAsync_ShouldPropagateRoutingException_WhenRoutingFails()
        {
            // Arrange
            var newTour = new TourDomain
            {
                Id = Guid.NewGuid(),
                Name = "New Tour",
                From = "Invalid Location",
                To = "Another Invalid",
                TransportType = "Car"
            };

            var routingException = new RoutingException("Failed to route");
            _routeService.GetRouteInfoAsync(newTour.From, newTour.To, newTour.TransportType).Throws(routingException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<RoutingException>(() => _tourService.AddTourAsync(newTour));
            Assert.That(ex, Is.SameAs(routingException));
        }

        [Test]
        public void AddTourAsync_ShouldWrappRepositoryException_WhenRepositoryFails()
        {
            // Arrange
            var newTour = new TourDomain
            {
                Id = Guid.NewGuid(),
                Name = "New Tour",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            var routeInfo = new RouteInfo
            {
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            var repositoryException = new TourRepositoryException("DB error");

            _routeService.GetRouteInfoAsync(newTour.From, newTour.To, newTour.TransportType)
                .Returns(routeInfo);

            _mapper.ToEntity(Arg.Any<TourDomain>()).Returns(new TourEntity());

            _tourRepository.AddTourAsync(Arg.Any<TourEntity>()).Throws(repositoryException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceException>(() => _tourService.AddTourAsync(newTour));
            Assert.That(ex.Message, Is.EqualTo("Failed to add the tour"));
            Assert.That(ex.InnerException, Is.SameAs(repositoryException));
        }

        [Test]
        public async Task DeleteTourAsync_ShouldDeleteAndReturnTrue_WhenTourExists()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            _tourRepository.DeleteTourAsync(tourId).Returns(true);

            // Act
            var result = await _tourService.DeleteTourAsync(tourId);

            // Assert
            Assert.That(result, Is.True);
            await _tourRepository.Received(1).DeleteTourAsync(tourId);
        }

        [Test]
        public async Task DeleteTourAsync_ShouldReturnFalse_WhenTourDoesNotExist()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            _tourRepository.DeleteTourAsync(tourId).Returns(false);

            // Act
            var result = await _tourService.DeleteTourAsync(tourId);

            // Assert
            Assert.That(result, Is.False);
            await _tourRepository.Received(1).DeleteTourAsync(tourId);
        }

        [Test]
        public void DeleteTourAsync_ShouldWrapRepositoryException_WhenRepositoryFails()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var repositoryException = new TourRepositoryException("DB error");
            _tourRepository.DeleteTourAsync(tourId).Throws(repositoryException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceException>(() => _tourService.DeleteTourAsync(tourId));
            Assert.That(ex.Message, Is.EqualTo($"Failed to delete tour with ID {tourId}."));
            Assert.That(ex.InnerException, Is.SameAs(repositoryException));
        }

        [Test]
        public async Task UpdateTourAsync_ShouldUpdateAndReturnTour_WhenEverythingIsCorrect()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Id = Guid.Empty,
                Name = "Updated Tour",
                Description = "Updated Description",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car"
            };

            var routeInfo = new RouteInfo
            {
                Distance = 200.5,
                EstimatedTime = 2.5,
                Directions = "Updated directions",
                GeoJson = "GeoJson string"
            };

            var tourEntity = new TourEntity
            {
                Id = tourId,
                Name = "Updated Tour",
                Description = "Updated Description",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car",
                Distance = 200.5,
                EstimatedTime = 2.5,
                Directions = "Updated directions",
                GeoJson = "GeoJson string"
            };

            var updatedTourEntity = new TourEntity
            {
                Id = tourId,
                Name = "Updated Tour",
                Description = "Updated Description",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car",
                Distance = 200.5,
                EstimatedTime = 2.5,
                Directions = "Updated directions",
                GeoJson = "GeoJson string"
            };

            var expectedTourDomain = new TourDomain
            {
                Id = tourId,
                Name = "Updated Tour",
                Description = "Updated Description",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car",
                Distance = 200.5,
                EstimatedTime = 2.5,
                Directions = "Updated directions",
                GeoJson = "GeoJson string"
            };

            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Returns(routeInfo);

            _mapper.ToEntity(Arg.Is<TourDomain>(d =>
                d.Id == tourId && d.Name == "Updated Tour")).Returns(tourEntity);

            _tourRepository.UpdateTourAsync(tourEntity).Returns(updatedTourEntity);

            _mapper.ToDomain(updatedTourEntity).Returns(expectedTourDomain);

            // Act
            var result = await _tourService.UpdateTourAsync(tourId, updatedTour);

            // Assert
            Assert.That(result, Is.EqualTo(expectedTourDomain));
            await _routeService.Received(1).GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType);
            await _tourRepository.Received(1).UpdateTourAsync(tourEntity);
        }

        [Test]
        public void UpdateTourAsync_ShouldPropagateRoutingException_WhenRoutingFails()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Name = "Updated Tour",
                From = "Invalid Location",
                To = "Another Invalid",
                TransportType = "Car"
            };

            var routingException = new RoutingException("Failed to route");
            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Throws(routingException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<RoutingException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));
            Assert.That(ex, Is.SameAs(routingException));
        }

        [Test]
        public void UpdateTourAsync_ShouldWrapNotFoundException_WhenTourDoesNotExist()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Name = "Updated Tour",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            var routeInfo = new RouteInfo
            {
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            var notFoundException = new TourNotFoundException($"Tour not found with ID {tourId}");

            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Returns(routeInfo);

            _mapper.ToEntity(Arg.Any<TourDomain>()).Returns(new TourEntity());

            _tourRepository.UpdateTourAsync(Arg.Any<TourEntity>()).Throws(notFoundException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceNotFoundException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));
            Assert.That(ex.Message, Is.EqualTo($"Tour with ID {tourId} not found."));
            Assert.That(ex.InnerException, Is.SameAs(notFoundException));
        }

        [Test]
        public void UpdateTourAsync_ShouldWrapRepositoryException_WhenRepositoryFails()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Name = "Updated Tour",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            var routeInfo = new RouteInfo
            {
                Distance = 300.5,
                EstimatedTime = 3.5,
                Directions = "Go straight",
                GeoJson = "GeoJson string"
            };

            var repositoryException = new TourRepositoryException("DB error");

            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Returns(routeInfo);

            _mapper.ToEntity(Arg.Any<TourDomain>()).Returns(new TourEntity());

            _tourRepository.UpdateTourAsync(Arg.Any<TourEntity>()).Throws(repositoryException);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourServiceException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));
            Assert.That(ex.Message, Is.EqualTo($"Error updating tour with ID {tourId}."));
            Assert.That(ex.InnerException, Is.SameAs(repositoryException));
        }

        [Test]
        public void UpdateTourAsync_ShouldThrowTourNameAlreadyExistsException_WhenNameExists()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Name = "Duplicate Tour Name",
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car"
            };

            // Setup repository to return that name already exists for a different tour
            _tourRepository.TourNameExistsAsync(updatedTour.Name, tourId).Returns(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<TourNameAlreadyExistsException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));

            Assert.That(ex.Message, Is.EqualTo($"A tour with the name '{updatedTour.Name}' already exists."));

            // Verify repository was called with correct parameters including tourId
            _tourRepository.Received(1).TourNameExistsAsync(updatedTour.Name, tourId);

            // Verify that routing service was never called (early return)
            _routeService.DidNotReceive().GetRouteInfoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
