using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Interfaces;
using TourPlanner.BLL.Exceptions;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.Exceptions;
using TourPlanner.Logging;

namespace TourPlanner.BLL.Services
{
    public class TourLogService : ITourLogService
    {
        private readonly ITourLogRepository _tourLogRepository;
        private readonly IMapper _mapper;
        private readonly ILoggerWrapper<TourLogService> _logger;

        public TourLogService(
            ITourLogRepository tourLogRepository,
            IMapper mapper,
            ILoggerWrapper<TourLogService> logger)
        {
            _tourLogRepository = tourLogRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TourLogDomain?> GetTourLogByIdAsync(Guid id)
        {
            _logger.LogDebug($"Getting tour log with ID: {id}");
            try
            {
                var entity = await _tourLogRepository.GetTourLogByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogInformation($"Tour log with ID: {id} not found");
                    return null;
                }

                return _mapper.ToDomain(entity);
            }
            catch (TourLogRepositoryException ex)
            {
                _logger.LogError(ex, $"Repository error retrieving tour log with ID: {id}");
                throw new TourLogServiceException($"Failed to retrieve tour log with ID {id}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error retrieving tour log with ID: {id}");
                throw new TourLogServiceException($"Unexpected error retrieving tour log with ID {id}", ex);
            }
        }

        public async Task<TourLogDomain> AddLogAsync(TourLogDomain newLog)
        {
            _logger.LogDebug($"Adding tour log for tour ID: {newLog.TourId}");

            if (newLog.Id == Guid.Empty)
            {
                newLog.Id = Guid.NewGuid();
            }

            try
            {
                var entity = _mapper.ToEntity(newLog);
                var savedEntity = await _tourLogRepository.AddLogAsync(entity);
                var result = _mapper.ToDomain(savedEntity);

                _logger.LogInformation($"Added tour log with ID: {result.Id} for tour ID: {result.TourId}");
                return result;
            }
            catch (TourLogRepositoryException ex)
            {
                _logger.LogError(ex, $"Repository error adding tour log for tour ID: {newLog.TourId}");
                throw new TourLogServiceException($"Failed to create tour log with ID {newLog.Id}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error adding tour log for tour ID: {newLog.TourId}");
                throw new TourLogServiceException($"Unexpected error creating tour log", ex);
            }
        }

        public async Task<bool> DeleteLogAsync(Guid id)
        {
            _logger.LogDebug($"Deleting tour log with ID: {id}");
            try
            {
                var result = await _tourLogRepository.DeleteLogAsync(id);
                _logger.LogInformation($"Tour log deletion result for ID: {id}: {(result ? "Deleted" : "Not Found")}");
                return result;
            }
            catch (TourLogRepositoryException ex)
            {
                _logger.LogError(ex, $"Repository error deleting tour log with ID: {id}");
                throw new TourLogServiceException($"Failed to delete tour log with ID {id}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error deleting tour log with ID: {id}");
                throw new TourLogServiceException($"Unexpected error deleting tour log with ID {id}", ex);
            }
        }

        public async Task<TourLogDomain> UpdateTourLogAsync(Guid id, TourLogDomain updatedTourLog)
        {
            _logger.LogDebug($"Updating tour log with ID: {id}");
            updatedTourLog.Id = id;

            try
            {
                var tourEntity = _mapper.ToEntity(updatedTourLog);
                var updatedEntity = await _tourLogRepository.UpdateTourLogAsync(tourEntity);
                var result = _mapper.ToDomain(updatedEntity);

                _logger.LogInformation($"Updated tour log with ID: {id}");
                return result;
            }
            catch (TourLogNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Tour log with ID: {id} not found for update");
                throw new TourLogServiceNotFoundException($"TourLog with ID {id} not found.", ex);
            }
            catch (TourLogRepositoryException ex)
            {
                _logger.LogError(ex, $"Repository error updating tour log with ID: {id}");
                throw new TourLogServiceException($"Error occurred while updating the TourLog with ID {id}.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error updating tour log with ID: {id}");
                throw new TourLogServiceException($"Unexpected error updating tour log with ID {id}", ex);
            }
        }
    }
}
