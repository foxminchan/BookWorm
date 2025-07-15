namespace BookWorm.AsyncAPI.Services;

/// <summary>
/// Service interface for aggregating AsyncAPI specifications from discovered services
/// </summary>
public interface IAsyncApiAggregatorService
{
    /// <summary>
    /// Gets aggregated AsyncAPI specification from all discovered services
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated AsyncAPI specification as JSON</returns>
    Task<string> GetAggregatedAsyncApiAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets list of discovered services with their AsyncAPI endpoints
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of service information</returns>
    Task<IEnumerable<ServiceInfo>> GetDiscoveredServicesAsync(CancellationToken cancellationToken = default);
}