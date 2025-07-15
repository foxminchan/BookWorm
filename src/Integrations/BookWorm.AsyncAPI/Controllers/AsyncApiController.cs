using BookWorm.AsyncAPI.Services;

namespace BookWorm.AsyncAPI.Controllers;

/// <summary>
/// Controller for exposing aggregated AsyncAPI specifications
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AsyncApiController : ControllerBase
{
    private readonly IAsyncApiAggregatorService _aggregatorService;
    private readonly ILogger<AsyncApiController> _logger;

    public AsyncApiController(
        IAsyncApiAggregatorService aggregatorService,
        ILogger<AsyncApiController> logger)
    {
        _aggregatorService = aggregatorService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the aggregated AsyncAPI specification from all services
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated AsyncAPI specification</returns>
    [HttpGet("aggregated")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAggregatedAsyncApi(CancellationToken cancellationToken)
    {
        try
        {
            var aggregatedSpec = await _aggregatorService.GetAggregatedAsyncApiAsync(cancellationToken);
            return Content(aggregatedSpec, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get aggregated AsyncAPI specification");
            return StatusCode(500, new { error = "Failed to aggregate AsyncAPI specifications" });
        }
    }

    /// <summary>
    /// Gets information about discovered services
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of discovered services</returns>
    [HttpGet("services")]
    [Produces("application/json")]
    public async Task<IActionResult> GetDiscoveredServices(CancellationToken cancellationToken)
    {
        try
        {
            var services = await _aggregatorService.GetDiscoveredServicesAsync(cancellationToken);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get discovered services");
            return StatusCode(500, new { error = "Failed to discover services" });
        }
    }
}