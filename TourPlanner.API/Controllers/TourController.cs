using Microsoft.AspNetCore.Mvc;
using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Exceptions;
using TourPlanner.BLL.Interfaces;
using TourPlanner.Logging;

namespace TourPlanner.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ILoggerWrapper<TourController> _logger;

        public TourController(ITourService tourService, ILoggerWrapper<TourController> logger)
        {
            _tourService = tourService;
            _logger = logger;
        }

        [HttpGet(Name = "GetTours")]
        public async Task<IEnumerable<TourDomain>> Get()
        {
            _logger.LogInformation("Getting all tours");
            try
            {
                var tours = await _tourService.GetAllToursAsync();
                _logger.LogInformation($"Retrieved {tours?.Count() ?? 0} tours");
                return tours ?? Enumerable.Empty<TourDomain>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving all tours");
                throw;
            }
        }

        [HttpPost(Name = "AddNewTour")]
        public async Task<ActionResult<TourDomain>> Post([FromBody] TourDomain newTour)
        {
            _logger.LogInformation($"Creating new tour: {newTour.Name}");
            try
            {
                var created = await _tourService.AddTourAsync(newTour);
                _logger.LogInformation($"Successfully created tour with ID: {created.Id}");
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (GeocodingException ex)
            {
                _logger.LogWarning(ex, $"Geocoding failed during tour creation for tour: {newTour.Name}");
                return BadRequest("Failed to geocode the provided locations.");
            }
            catch (RoutingException ex)
            {
                _logger.LogWarning(ex, $"Routing failed during tour creation for tour: {newTour.Name}");
                return StatusCode(502, "Unable to retrieve routing information.");
            }
            catch (TourNameAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, $"Tour with name '{newTour.Name}' already exists.");
                return Conflict(ex.Message);
            }
            catch (TourServiceException ex)
            {
                _logger.LogError(ex, $"Tour service error creating new tour: {newTour.Name}");
                return StatusCode(500, "An internal service error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error creating new tour: {newTour.Name}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpDelete("{id:guid}", Name = "DeleteTour")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug($"Deleting tour with ID: {id}");
            try
            {
                var deleted = await _tourService.DeleteTourAsync(id);
                if (!deleted)
                {
                    _logger.LogInformation($"Tour with ID: {id} not found for deletion");
                    return NotFound();
                }

                _logger.LogInformation($"Successfully deleted tour with ID: {id}");
                return NoContent();
            }
            catch (TourServiceException ex)
            {
                _logger.LogError(ex, $"Tour service error during deletion of tour with ID: {id}");
                return StatusCode(500, "An internal service error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error deleting tour with ID: {id}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id:guid}", Name = "UpdateTour")]
        public async Task<ActionResult<TourDomain>> Put(Guid id, [FromBody] TourDomain updatedTour)
        {
            _logger.LogDebug($"Updating tour with ID: {id}");
            try
            {
                var result = await _tourService.UpdateTourAsync(id, updatedTour);
                _logger.LogInformation($"Successfully updated tour with ID: {id}");
                return Ok(result);
            }
            catch (GeocodingException ex)
            {
                _logger.LogWarning(ex, $"Geocoding failed during update of tour with ID: {id}");
                return BadRequest("Failed to geocode the provided locations.");
            }
            catch (RoutingException ex)
            {
                _logger.LogWarning(ex, $"Routing failed during update of tour with ID: {id}");
                return StatusCode(502, "Unable to retrieve routing information.");
            }
            catch (TourServiceNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Tour with ID: {id} not found for update");
                return NotFound(ex.Message);
            }
            catch (TourNameAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, $"Tour with name '{updatedTour.Name}' already exists during update of tour with ID: {id}");
                return Conflict(ex.Message);
            }
            catch (TourServiceException ex)
            {
                _logger.LogError(ex, $"Tour service error during update of tour with ID: {id}");
                return StatusCode(500, "An internal service error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error updating tour with ID: {id}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportTours([FromBody] List<TourDomain> tours)
        {
            _logger.LogInformation($"Importing {tours.Count} tours");
            try
            {
                var importedCount = await _tourService.ImportToursAsync(tours);

                _logger.LogInformation($"Import completed: {importedCount} tours imported");
                return Ok(new { ImportedCount = importedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tour import");
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
