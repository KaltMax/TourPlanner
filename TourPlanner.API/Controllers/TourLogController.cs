using Microsoft.AspNetCore.Mvc;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Interfaces;
using TourPlanner.BLL.Exceptions;
using TourPlanner.Logging;

namespace TourPlanner.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TourLogController : ControllerBase
    {
        private readonly ITourLogService _tourLogService;
        private readonly ILoggerWrapper<TourLogController> _logger;

        public TourLogController(ITourLogService tourLogService, ILoggerWrapper<TourLogController> logger)
        {
            _tourLogService = tourLogService;
            _logger = logger;
        }

        [HttpGet("{id:guid}", Name = "GetById")]
        public async Task<ActionResult<TourLogDomain>> GetById(Guid id)
        {
            _logger.LogDebug($"Getting tour log with ID: {id}");
            try
            {
                var tourLog = await _tourLogService.GetTourLogByIdAsync(id);
                if (tourLog == null)
                {
                    _logger.LogInformation($"TourLog with ID {id} not found");
                    return NotFound();
                }

                _logger.LogInformation($"Successfully retrieved tour log with ID: {id}");
                return Ok(tourLog);
            }
            catch (TourLogServiceException ex)
            {
                _logger.LogError(ex, $"Error retrieving tour log with ID {id}");
                return StatusCode(500, "An internal error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error retrieving tour log with ID {id}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost(Name = "AddNewTourLog")]
        public async Task<ActionResult<TourLogDomain>> Post([FromBody] TourLogDomain newLog)
        {
            _logger.LogDebug($"Creating new tour log for tour ID: {newLog.TourId}");
            try
            {
                var created = await _tourLogService.AddLogAsync(newLog);
                _logger.LogInformation($"Created tour log with ID: {created.Id} for tour ID: {created.TourId}");
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (TourLogServiceException ex)
            {
                _logger.LogError(ex, $"Error creating new tour log for tour ID: {newLog.TourId}");
                return StatusCode(500, "An internal error occurred while creating the tour log.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error creating new tour log for tour ID: {newLog.TourId}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id:guid}", Name = "DeleteTourLog")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug($"Deleting tour log with ID: {id}");
            try
            {
                var deleted = await _tourLogService.DeleteLogAsync(id);
                if (!deleted)
                {
                    _logger.LogInformation($"Tour log with ID: {id} not found for deletion");
                    return NotFound();
                }

                _logger.LogInformation($"Deleted tour log with ID: {id}");
                return NoContent();
            }
            catch (TourLogServiceException ex)
            {
                _logger.LogError(ex, $"Error deleting tour log with ID {id}");
                return StatusCode(500, "An internal error occurred while deleting the tour log.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error deleting tour log with ID {id}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id:guid}", Name = "UpdateTourLog")]
        public async Task<ActionResult<TourLogDomain>> Put(Guid id, [FromBody] TourLogDomain updatedTourLog)
        {
            _logger.LogDebug($"Updating tour log with ID: {id}");
            try
            {
                var result = await _tourLogService.UpdateTourLogAsync(id, updatedTourLog);
                _logger.LogInformation($"Updated tour log with ID: {id}");
                return Ok(result);
            }
            catch (TourLogServiceNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Tour log with ID: {id} not found for update");
                return NotFound(ex.Message);
            }
            catch (TourLogServiceException ex)
            {
                _logger.LogError(ex, $"Service-level error updating tour log with ID: {id}");
                return StatusCode(500, "An internal error occurred while updating the tour log.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error updating tour log with ID: {id}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}