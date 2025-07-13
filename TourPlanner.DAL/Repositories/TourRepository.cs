using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.Context;
using TourPlanner.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using TourPlanner.DAL.Exceptions;
using TourPlanner.Logging;

namespace TourPlanner.DAL.Repositories
{
    public class TourRepository : ITourRepository
    {
        private readonly TourPlannerDbContext _context;
        private readonly ILoggerWrapper<TourRepository> _logger;

        public TourRepository(TourPlannerDbContext context, ILoggerWrapper<TourRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TourEntity>> GetAllToursAsync()
        {
            _logger.LogInformation("Retrieving all tours from database");
            try
            {
                var tours = await _context.Tours
                    .Include(t => t.TourLogs)
                    .ToListAsync();
                _logger.LogInformation($"Retrieved {tours.Count} tours from database");
                return tours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all tours from database");
                throw;
            }
        }

        public async Task<TourEntity> AddTourAsync(TourEntity newTourEntity)
        {
            _logger.LogInformation($"Adding new tour to database: {newTourEntity.Id} - {newTourEntity.Name}");
            try
            {
                var savedEntity = _context.Tours.Add(newTourEntity).Entity;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully added tour: {newTourEntity.Id}");
                return savedEntity;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error adding tour: {newTourEntity.Id}");
                throw new TourRepositoryException("Error saving tour to the database.", ex);
            }
        }

        public async Task<bool> DeleteTourAsync(Guid id)
        {
            _logger.LogInformation($"Deleting tour from database: {id}");
            try
            {
                var entity = await _context.Tours.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    _logger.LogInformation($"Tour to delete not found: {id}");
                    return false;
                }

                _context.Tours.Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully deleted tour: {id}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error deleting tour: {id}");
                throw new TourRepositoryException("Error deleting tour from the database", ex);
            }
        }

        public async Task<TourEntity> UpdateTourAsync(TourEntity updatedTourEntity)
        {
            _logger.LogInformation($"Updating tour in database: {updatedTourEntity.Id}");

            var existing = await _context.Tours.FirstOrDefaultAsync(t => t.Id == updatedTourEntity.Id);
            if (existing == null)
            {
                _logger.LogWarning($"Tour to update not found: {updatedTourEntity.Id}");
                throw new TourNotFoundException(updatedTourEntity.Id);
            }

            existing.Name = updatedTourEntity.Name;
            existing.Description = updatedTourEntity.Description;
            existing.From = updatedTourEntity.From;
            existing.To = updatedTourEntity.To;
            existing.GeoJson = updatedTourEntity.GeoJson;
            existing.Directions = updatedTourEntity.Directions;
            existing.Distance = updatedTourEntity.Distance;
            existing.EstimatedTime = updatedTourEntity.EstimatedTime;
            existing.TransportType = updatedTourEntity.TransportType;
            existing.TourLogs = updatedTourEntity.TourLogs;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated tour: {updatedTourEntity.Id}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error updating tour: {updatedTourEntity.Id}");
                throw new TourRepositoryException("Error updating tour in the database.", ex);
            }

            return existing;
        }

        public async Task<bool> TourNameExistsAsync(string name, Guid? excludeId = null)
        {
            _logger.LogDebug($"Checking if tour name exists: {name} (excluding ID: {excludeId})");
            try
            {
                var query = _context.Tours.AsQueryable();

                if (excludeId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeId.Value);
                }

                var exists = await query.AnyAsync(t => t.Name.ToLower() == name.ToLower());
                _logger.LogDebug($"Tour name '{name}' exists: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if tour name exists: {name}");
                throw new TourRepositoryException($"Error checking if tour name exists: {name}", ex);
            }
        }
    }
}
