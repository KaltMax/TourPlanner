using Microsoft.EntityFrameworkCore;
using TourPlanner.DAL.Context;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Exceptions;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Logging;

namespace TourPlanner.DAL.Repositories
{
    public class TourLogRepository : ITourLogRepository
    {
        private readonly TourPlannerDbContext _context;
        private readonly ILoggerWrapper<TourLogRepository> _logger;

        public TourLogRepository(TourPlannerDbContext context, ILoggerWrapper<TourLogRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TourLogEntity?> GetTourLogByIdAsync(Guid id)
        {
            _logger.LogDebug($"Retrieving tour log from database: {id}");
            try
            {
                var result = await _context.TourLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(log => log.Id == id);

                if (result == null)
                {
                    _logger.LogInformation($"Tour log not found in database: {id}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Database error retrieving tour log: {id}");
                throw;
            }
        }

        public async Task<TourLogEntity> AddLogAsync(TourLogEntity newLog)
        {
            _logger.LogDebug($"Adding tour log to database: {newLog.Id} for tour: {newLog.TourId}");
            try
            {
                var saved = _context.TourLogs.Add(newLog).Entity;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added tour log to database: {newLog.Id}");
                return saved;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error saving tour log: {newLog.Id}");
                throw new TourLogRepositoryException($"Error saving the new tour log with ID {newLog.Id}.", ex);
            }
        }

        public async Task<bool> DeleteLogAsync(Guid id)
        {
            _logger.LogDebug($"Deleting tour log from database: {id}");
            try
            {
                var log = await _context.TourLogs.FirstOrDefaultAsync(l => l.Id == id);
                if (log == null)
                {
                    _logger.LogInformation($"Tour log to delete not found in database: {id}");
                    return false;
                }

                _context.TourLogs.Remove(log);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted tour log from database: {id}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error deleting tour log: {id}");
                throw new TourLogRepositoryException($"Error deleting the tour log with ID {id}.", ex);
            }
        }

        public async Task<TourLogEntity> UpdateTourLogAsync(TourLogEntity updatedTourLogEntity)
        {
            _logger.LogDebug($"Updating tour log in database: {updatedTourLogEntity.Id}");

            var existing = await _context.TourLogs.FirstOrDefaultAsync(log => log.Id == updatedTourLogEntity.Id);
            if (existing == null)
            {
                _logger.LogWarning($"Tour log to update not found in database: {updatedTourLogEntity.Id}");
                throw new TourLogNotFoundException(updatedTourLogEntity.Id);
            }

            existing.Comment = updatedTourLogEntity.Comment;
            existing.TotalDistance = updatedTourLogEntity.TotalDistance;
            existing.TotalTime = updatedTourLogEntity.TotalTime;
            existing.Difficulty = updatedTourLogEntity.Difficulty;
            existing.Rating = updatedTourLogEntity.Rating;
            existing.Date = updatedTourLogEntity.Date;
            existing.TourId = updatedTourLogEntity.TourId;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated tour log in database: {updatedTourLogEntity.Id}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error updating tour log: {updatedTourLogEntity.Id}");
                throw new TourLogRepositoryException($"Error updating the tour log with ID {updatedTourLogEntity.Id}.", ex);
            }

            return existing;
        }
    }
}
