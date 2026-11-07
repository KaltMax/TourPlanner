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
        public async Task GetAllToursAsync_ShouldPropagateException_WhenRepositoryThrows()
        {
            // Arrange
            var expectedException = new Exception("Repository error");
            _tourRepository.GetAllToursAsync().Throws(expectedException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _tourService.GetAllToursAsync());
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
                d != null && d.Id == newTourId && d.Name == "New Tour")).Returns(tourEntity);

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
        public async Task AddTourAsync_ShouldThrowTourNameAlreadyExistsException_WhenNameExists()
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
            var ex = await Assert.ThrowsAsync<TourNameAlreadyExistsException>(() => _tourService.AddTourAsync(newTour));

            Assert.That(ex.Message, Is.EqualTo($"A tour with the name '{newTour.Name}' already exists."));

            // Verify the repository was called with correct parameters
            await _tourRepository.Received(1).TourNameExistsAsync(newTour.Name, newTour.Id);

            // Verify that routing service was never called (early return)
            await _routeService.DidNotReceive().GetRouteInfoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public async Task AddTourAsync_ShouldPropagateRoutingException_WhenRoutingFails()
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
            var ex = await Assert.ThrowsAsync<RoutingException>(() => _tourService.AddTourAsync(newTour));
            Assert.That(ex, Is.SameAs(routingException));
        }

        [Test]
        public async Task AddTourAsync_ShouldWrappRepositoryException_WhenRepositoryFails()
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
            var ex = await Assert.ThrowsAsync<TourServiceException>(() => _tourService.AddTourAsync(newTour));
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
        public async Task DeleteTourAsync_ShouldWrapRepositoryException_WhenRepositoryFails()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var repositoryException = new TourRepositoryException("DB error");
            _tourRepository.DeleteTourAsync(tourId).Throws(repositoryException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourServiceException>(() => _tourService.DeleteTourAsync(tourId));
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

            // Existing tour has a different destination, so the route must be recalculated.
            _tourRepository.GetTourByIdAsync(tourId).Returns(new TourEntity
            {
                Id = tourId,
                From = "Vienna",
                To = "Salzburg",
                TransportType = "Car",
                GeoJson = "old",
                Directions = "old",
                Distance = 100
            });

            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Returns(routeInfo);

            _mapper.ToEntity(Arg.Is<TourDomain>(d =>
                d != null && d.Id == tourId && d.Name == "Updated Tour")).Returns(tourEntity);

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
        public async Task UpdateTourAsync_ShouldReuseExistingRouteData_WhenRouteInputsUnchanged()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Id = tourId,
                Name = "Renamed Tour",
                Description = "Only metadata changed",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car"
            };

            // Same route inputs as the update, with complete route data already stored.
            var existing = new TourEntity
            {
                Id = tourId,
                Name = "Old Name",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car",
                GeoJson = "stored-geojson",
                Directions = "stored-directions",
                Distance = 123.4,
                EstimatedTime = 45.6
            };
            _tourRepository.GetTourByIdAsync(tourId).Returns(existing);

            TourDomain? mappedDomain = null;
            _mapper.ToEntity(Arg.Do<TourDomain>(d => mappedDomain = d)).Returns(new TourEntity());
            _tourRepository.UpdateTourAsync(Arg.Any<TourEntity>()).Returns(new TourEntity());
            _mapper.ToDomain(Arg.Any<TourEntity>()).Returns(new TourDomain { Id = tourId });

            // Act
            await _tourService.UpdateTourAsync(tourId, updatedTour);

            // Assert: no external routing call, and the stored route data is carried over.
            await _routeService.DidNotReceive().GetRouteInfoAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
            Assert.That(mappedDomain, Is.Not.Null);
            Assert.That(mappedDomain!.GeoJson, Is.EqualTo("stored-geojson"));
            Assert.That(mappedDomain.Directions, Is.EqualTo("stored-directions"));
            Assert.That(mappedDomain.Distance, Is.EqualTo(123.4));
            Assert.That(mappedDomain.EstimatedTime, Is.EqualTo(45.6));
        }

        [Test]
        public async Task UpdateTourAsync_ShouldRecalculateRoute_WhenStoredRouteDataIncomplete()
        {
            // Arrange
            var tourId = Guid.NewGuid();
            var updatedTour = new TourDomain
            {
                Id = tourId,
                Name = "Renamed Tour",
                From = "Vienna",
                To = "Graz",
                TransportType = "Car"
            };

            // Same route inputs, but the stored route data is missing, so it must be re-fetched.
            var existing = new TourEntity
            {
                Id = tourId,
                From = "Vienna",
                To = "Graz",
                TransportType = "Car",
                GeoJson = "",
                Directions = "",
                Distance = 0
            };
            _tourRepository.GetTourByIdAsync(tourId).Returns(existing);

            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Returns(new RouteInfo { Distance = 10, EstimatedTime = 20, Directions = "d", GeoJson = "g" });
            _mapper.ToEntity(Arg.Any<TourDomain>()).Returns(new TourEntity());
            _tourRepository.UpdateTourAsync(Arg.Any<TourEntity>()).Returns(new TourEntity());
            _mapper.ToDomain(Arg.Any<TourEntity>()).Returns(new TourDomain { Id = tourId });

            // Act
            await _tourService.UpdateTourAsync(tourId, updatedTour);

            // Assert
            await _routeService.Received(1).GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType);
        }

        [Test]
        public async Task UpdateTourAsync_ShouldPropagateRoutingException_WhenRoutingFails()
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

            // Existing tour differs, so routing is attempted (and fails).
            _tourRepository.GetTourByIdAsync(tourId).Returns(new TourEntity
            {
                Id = tourId,
                From = "Old From",
                To = "Old To",
                TransportType = "Car",
                GeoJson = "old",
                Directions = "old",
                Distance = 100
            });

            var routingException = new RoutingException("Failed to route");
            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Throws(routingException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RoutingException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));
            Assert.That(ex, Is.SameAs(routingException));
        }

        [Test]
        public async Task UpdateTourAsync_ShouldThrowNotFoundException_WhenTourDoesNotExist()
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

            _tourRepository.GetTourByIdAsync(tourId).Returns((TourEntity?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourServiceNotFoundException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));
            Assert.That(ex.Message, Is.EqualTo($"Tour with ID {tourId} not found."));

            // The routing provider must not be contacted for a tour that does not exist.
            await _routeService.DidNotReceive().GetRouteInfoAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task UpdateTourAsync_ShouldWrapNotFoundException_WhenRepositoryThrowsNotFound()
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

            // The tour exists at read time but is deleted before the write (a race).
            _tourRepository.GetTourByIdAsync(tourId).Returns(new TourEntity
            {
                Id = tourId,
                From = "Old From",
                To = "Old To",
                TransportType = "Car",
                GeoJson = "old",
                Directions = "old",
                Distance = 100
            });

            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Returns(routeInfo);

            _mapper.ToEntity(Arg.Any<TourDomain>()).Returns(new TourEntity());

            _tourRepository.UpdateTourAsync(Arg.Any<TourEntity>()).Throws(notFoundException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourServiceNotFoundException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));
            Assert.That(ex.Message, Is.EqualTo($"Tour with ID {tourId} not found."));
            Assert.That(ex.InnerException, Is.SameAs(notFoundException));
        }

        [Test]
        public async Task UpdateTourAsync_ShouldWrapRepositoryException_WhenRepositoryFails()
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

            _tourRepository.GetTourByIdAsync(tourId).Returns(new TourEntity
            {
                Id = tourId,
                From = "Old From",
                To = "Old To",
                TransportType = "Car",
                GeoJson = "old",
                Directions = "old",
                Distance = 100
            });

            _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType)
                .Returns(routeInfo);

            _mapper.ToEntity(Arg.Any<TourDomain>()).Returns(new TourEntity());

            _tourRepository.UpdateTourAsync(Arg.Any<TourEntity>()).Throws(repositoryException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TourServiceException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));
            Assert.That(ex.Message, Is.EqualTo($"Error updating tour with ID {tourId}."));
            Assert.That(ex.InnerException, Is.SameAs(repositoryException));
        }

        [Test]
        public async Task UpdateTourAsync_ShouldThrowTourNameAlreadyExistsException_WhenNameExists()
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
            var ex = await Assert.ThrowsAsync<TourNameAlreadyExistsException>(() => _tourService.UpdateTourAsync(tourId, updatedTour));

            Assert.That(ex.Message, Is.EqualTo($"A tour with the name '{updatedTour.Name}' already exists."));

            // Verify repository was called with correct parameters including tourId
            await _tourRepository.Received(1).TourNameExistsAsync(updatedTour.Name, tourId);

            // Verify that routing service was never called (early return)
            await _routeService.DidNotReceive().GetRouteInfoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
