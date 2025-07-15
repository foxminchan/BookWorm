namespace BookWorm.AsyncAPI.Services;

/// <summary>
/// Information about a discovered service
/// </summary>
public class ServiceInfo
{
    public string Name { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string AsyncApiUrl { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
}