using TourPlanner.BLL.Interfaces;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Exceptions;
using TourPlanner.DAL.Exceptions;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Logging;

namespace TourPlanner.BLL.Services
{
    public class TourService : ITourService
    {
        private readonly IRouteService _routeService;
        private readonly ITourRepository _tourRepository;
        private readonly IMapper _mapper;
        private readonly ILoggerWrapper<TourService> _logger;

        public TourService(IRouteService routeService,
            ITourRepository tourRepository,
            IMapper mapper,
            ILoggerWrapper<TourService> logger)
        {
            _routeService = routeService;
            _tourRepository = tourRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TourDomain>> GetAllToursAsync()
        {
            _logger.LogDebug("Getting all tours");
            try
            {
                var tourEntities = await _tourRepository.GetAllToursAsync();
                var tours = tourEntities.Select(e => _mapper.ToDomain(e)).ToList();
                return tours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tours");
                throw;
            }
        }

        public async Task<TourDomain> AddTourAsync(TourDomain newTour)
        {
            _logger.LogDebug($"Adding new tour: {newTour.Name}");

            if (newTour.Id == Guid.Empty)
            {
                newTour.Id = Guid.NewGuid();
            }

            try
            {
                if (await _tourRepository.TourNameExistsAsync(newTour.Name, newTour.Id))
                {
                    _logger.LogWarning($"Tour with name '{newTour.Name}' already exists.");
                    throw new TourNameAlreadyExistsException($"A tour with the name '{newTour.Name}' already exists.");
                }

                await FetchRouteDataAsync(newTour);

                var tourEntity = _mapper.ToEntity(newTour);
                var savedEntity = await _tourRepository.AddTourAsync(tourEntity);
                var result = _mapper.ToDomain(savedEntity);

                _logger.LogInformation($"Successfully added tour: {result.Name} with ID: {result.Id}");
                return result;
            }
            catch (RoutingException ex)
            {
                _logger.LogWarning(ex, $"Routing failed for tour: {newTour.Name}");
                throw;
            }
            catch (TourRepositoryException ex)
            {
                _logger.LogError(ex, $"Failed to add tour: {newTour.Name}");
                throw new TourServiceException("Failed to add the tour", ex);
            }
        }

        public async Task<bool> DeleteTourAsync(Guid id)
        {
            _logger.LogDebug($"Deleting tour with ID: {id}");

            try
            {
                var result = await _tourRepository.DeleteTourAsync(id);
                _logger.LogInformation($"Tour deletion result for ID {id}: {(result ? "Success" : "Not Found")}");
                return result;
            }
            catch (TourRepositoryException ex)
            {
                _logger.LogError(ex, $"Failed to delete tour with ID: {id}");
                throw new TourServiceException($"Failed to delete tour with ID {id}.", ex);
            }
        }

        public async Task<TourDomain> UpdateTourAsync(Guid id, TourDomain updatedTour)
        {
            _logger.LogDebug($"Updating tour with ID: {id}");

            updatedTour.Id = id;

            try
            {
                if (await _tourRepository.TourNameExistsAsync(updatedTour.Name, updatedTour.Id))
                {
                    _logger.LogWarning($"Tour with name '{updatedTour.Name}' already exists.");
                    throw new TourNameAlreadyExistsException($"A tour with the name '{updatedTour.Name}' already exists.");
                }

                var routeInfo = await _routeService.GetRouteInfoAsync(updatedTour.From, updatedTour.To, updatedTour.TransportType);
                updatedTour.Distance = routeInfo.Distance;
                updatedTour.EstimatedTime = routeInfo.EstimatedTime;
                updatedTour.Directions = routeInfo.Directions;
                updatedTour.GeoJson = routeInfo.GeoJson;

                var tourEntity = _mapper.ToEntity(updatedTour);
                var updatedEntity = await _tourRepository.UpdateTourAsync(tourEntity);
                var result = _mapper.ToDomain(updatedEntity);

                _logger.LogInformation($"Successfully updated tour with ID: {id}");
                return result;
            }
            catch (RoutingException ex)
            {
                _logger.LogWarning(ex, $"Routing failed during tour update with ID: {id}");
                throw;
            }
            catch (TourNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Tour with ID: {id} not found during update");
                throw new TourServiceNotFoundException($"Tour with ID {id} not found.", ex);
            }
            catch (TourRepositoryException ex)
            {
                _logger.LogError(ex, $"Error updating tour with ID: {id}");
                throw new TourServiceException($"Error updating tour with ID {id}.", ex);
            }
        }

        public async Task<int> ImportToursAsync(IEnumerable<TourDomain> tours)
        {
            _logger.LogDebug($"Importing {tours.Count()} tours");

            var importedCount = 0;

            foreach (var tour in tours)
            {
                try
                {
                    // Skip if tour already exists by name
                    if (await _tourRepository.TourNameExistsAsync(tour.Name))
                    {
                        _logger.LogInformation($"Tour '{tour.Name}' already exists, skipping");
                        continue;
                    }

                    // Generate new ID for import to avoid conflicts
                    tour.Id = Guid.NewGuid();

                    // Update TourLog IDs and references
                    foreach (var tourLog in tour.TourLogs)
                    {
                        tourLog.Id = Guid.NewGuid();
                        tourLog.TourId = tour.Id;
                    }

                    // Ensure route data exists
                    await EnsureRouteDataAsync(tour);

                    var tourEntity = _mapper.ToEntity(tour);
                    await _tourRepository.AddTourAsync(tourEntity);

                    importedCount++;
                    _logger.LogInformation($"Successfully imported tour: {tour.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to import tour: {tour.Name}");
                    // Continue with next tour instead of failing entire import
                }
            }

            _logger.LogInformation($"Import completed: {importedCount} tours imported");
            return importedCount;
        }

        private async Task EnsureRouteDataAsync(TourDomain tour)
        {
            // Check if route data already exists (for imported tours)
            if (string.IsNullOrEmpty(tour.GeoJson) ||
                string.IsNullOrEmpty(tour.Directions) ||
                tour.Distance <= 0)
            {
                _logger.LogDebug($"Route data missing for tour '{tour.Name}', fetching from routing service");
                await FetchRouteDataAsync(tour);
            }
            else
            {
                _logger.LogDebug($"Route data already exists for tour '{tour.Name}', skipping fetch");
            }
        }

        private async Task FetchRouteDataAsync(TourDomain tour)
        {
            var routeInfo = await _routeService.GetRouteInfoAsync(tour.From, tour.To, tour.TransportType);
            tour.Distance = routeInfo.Distance;
            tour.EstimatedTime = routeInfo.EstimatedTime;
            tour.Directions = routeInfo.Directions;
            tour.GeoJson = routeInfo.GeoJson;
        }
    }
}
