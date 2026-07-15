using Microsoft.AspNetCore.Mvc;
using TourPlanner.BLL.DomainModels;
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
        [ProducesResponseType(typeof(IEnumerable<TourDomain>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<TourDomain>> Get()
        {
            _logger.LogInformation("Getting all tours");

            var tours = await _tourService.GetAllToursAsync();
            _logger.LogInformation($"Retrieved {tours?.Count() ?? 0} tours");
            return tours ?? Enumerable.Empty<TourDomain>();
        }

        [HttpPost(Name = "AddNewTour")]
        [ProducesResponseType(typeof(TourDomain), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TourDomain>> Post([FromBody] TourDomain newTour, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Creating new tour: {newTour.Name}");

            var created = await _tourService.AddTourAsync(newTour, cancellationToken);
            _logger.LogInformation($"Successfully created tour with ID: {created.Id}");
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
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
        [ProducesResponseType(typeof(TourDomain), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TourDomain>> Put(Guid id, [FromBody] TourDomain updatedTour, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"Updating tour with ID: {id}");

            var result = await _tourService.UpdateTourAsync(id, updatedTour, cancellationToken);
            _logger.LogInformation($"Successfully updated tour with ID: {id}");
            return Ok(result);
        }

        [HttpPost("import")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportTours([FromBody] List<TourDomain> tours, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Importing {tours.Count} tours");

            var importedCount = await _tourService.ImportToursAsync(tours, cancellationToken);
            _logger.LogInformation($"Import completed: {importedCount} tours imported");
            return Ok(new { ImportedCount = importedCount });
        }
    }
}
