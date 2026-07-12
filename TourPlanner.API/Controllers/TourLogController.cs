using Microsoft.AspNetCore.Mvc;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Interfaces;
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
        [ProducesResponseType(typeof(TourLogDomain), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TourLogDomain>> GetById(Guid id)
        {
            _logger.LogDebug($"Getting tour log with ID: {id}");

            var tourLog = await _tourLogService.GetTourLogByIdAsync(id);
            if (tourLog == null)
            {
                _logger.LogInformation($"TourLog with ID {id} not found");
                return NotFound();
            }

            _logger.LogInformation($"Successfully retrieved tour log with ID: {id}");
            return Ok(tourLog);
        }

        [HttpPost(Name = "AddNewTourLog")]
        [ProducesResponseType(typeof(TourLogDomain), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TourLogDomain>> Post([FromBody] TourLogDomain newLog)
        {
            _logger.LogDebug($"Creating new tour log for tour ID: {newLog.TourId}");

            var created = await _tourLogService.AddLogAsync(newLog);
            _logger.LogInformation($"Created tour log with ID: {created.Id} for tour ID: {created.TourId}");
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:guid}", Name = "DeleteTourLog")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug($"Deleting tour log with ID: {id}");

            var deleted = await _tourLogService.DeleteLogAsync(id);
            if (!deleted)
            {
                _logger.LogInformation($"Tour log with ID: {id} not found for deletion");
                return NotFound();
            }

            _logger.LogInformation($"Deleted tour log with ID: {id}");
            return NoContent();
        }

        [HttpPut("{id:guid}", Name = "UpdateTourLog")]
        [ProducesResponseType(typeof(TourLogDomain), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TourLogDomain>> Put(Guid id, [FromBody] TourLogDomain updatedTourLog)
        {
            _logger.LogDebug($"Updating tour log with ID: {id}");

            var result = await _tourLogService.UpdateTourLogAsync(id, updatedTourLog);
            _logger.LogInformation($"Updated tour log with ID: {id}");
            return Ok(result);
        }
    }
}
