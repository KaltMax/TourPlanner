using Microsoft.AspNetCore.Mvc;
using TourPlanner.API.DTOs;
using TourPlanner.API.Mapping;
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
        [ProducesResponseType(typeof(IEnumerable<TourResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<TourResponse>> Get()
        {
            _logger.LogInformation("Getting all tours");

            var tours = await _tourService.GetAllToursAsync();
            var response = (tours)
                .Select(tour => tour.ToResponse())
                .ToList();
            _logger.LogInformation($"Retrieved {response.Count} tours");
            return response;
        }

        [HttpPost(Name = "AddNewTour")]
        [ProducesResponseType(typeof(TourResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TourResponse>> Post([FromBody] TourRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Creating new tour: {request.Name}");

            var created = await _tourService.AddTourAsync(request.ToDomain(), cancellationToken);
            _logger.LogInformation($"Successfully created tour with ID: {created.Id}");
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created.ToResponse());
        }

        [HttpDelete("{id:guid}", Name = "DeleteTour")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug($"Deleting tour with ID: {id}");

            var deleted = await _tourService.DeleteTourAsync(id);
            if (!deleted)
            {
                _logger.LogInformation($"Tour with ID: {id} not found for deletion");
                return NotFound();
            }

            _logger.LogInformation($"Successfully deleted tour with ID: {id}");
            return NoContent();
        }

        [HttpPut("{id:guid}", Name = "UpdateTour")]
        [ProducesResponseType(typeof(TourResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TourResponse>> Put(Guid id, [FromBody] TourRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"Updating tour with ID: {id}");

            var result = await _tourService.UpdateTourAsync(id, request.ToDomain(), cancellationToken);
            _logger.LogInformation($"Successfully updated tour with ID: {id}");
            return Ok(result.ToResponse());
        }

        [HttpPost("import")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportTours([FromBody] List<ImportTourRequest> tours, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Importing {tours.Count} tours");

            var domains = tours.Select(tour => tour.ToDomain()).ToList();
            var importedCount = await _tourService.ImportToursAsync(domains, cancellationToken);
            _logger.LogInformation($"Import completed: {importedCount} tours imported");
            return Ok(new { ImportedCount = importedCount });
        }
    }
}
