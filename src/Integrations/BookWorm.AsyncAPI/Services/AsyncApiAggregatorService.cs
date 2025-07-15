using System.Text.Json.Nodes;

namespace BookWorm.AsyncAPI.Services;

/// <summary>
/// Service for aggregating AsyncAPI specifications from discovered services
/// </summary>
public class AsyncApiAggregatorService : IAsyncApiAggregatorService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AsyncApiAggregatorService> _logger;
    
    // Known BookWorm services that should have AsyncAPI
    private readonly string[] _knownServices = 
    [
        Application.Catalog,
        Application.Basket,
        Application.Ordering,
        Application.Rating,
        Application.Finance,
        Application.Notification
    ];

    public AsyncApiAggregatorService(
        IServiceProvider serviceProvider,
        HttpClient httpClient,
        ILogger<AsyncApiAggregatorService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceInfo>> GetDiscoveredServicesAsync(CancellationToken cancellationToken = default)
    {
        var services = new List<ServiceInfo>();
        
        try
        {
            var serviceProvider = _serviceProvider.GetService<IServiceEndpointProvider>();
            if (serviceProvider is null)
            {
                _logger.LogWarning("Service endpoint provider not available");
                return services;
            }

            foreach (var serviceName in _knownServices)
            {
                try
                {
                    var endpoints = await serviceProvider.GetEndpointsAsync(serviceName, cancellationToken);
                    var endpoint = endpoints.FirstOrDefault();
                    
                    if (endpoint is not null)
                    {
                        var baseUrl = $"{endpoint.Scheme}://{endpoint.Host}:{endpoint.Port}";
                        var asyncApiUrl = $"{baseUrl}/asyncapi/docs/asyncapi.json";
                        
                        var isAvailable = await CheckServiceAvailabilityAsync(baseUrl, cancellationToken);
                        
                        services.Add(new ServiceInfo
                        {
                            Name = serviceName,
                            BaseUrl = baseUrl,
                            AsyncApiUrl = asyncApiUrl,
                            IsAvailable = isAvailable
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to discover service {ServiceName}", serviceName);
                    services.Add(new ServiceInfo
                    {
                        Name = serviceName,
                        BaseUrl = string.Empty,
                        AsyncApiUrl = string.Empty,
                        IsAvailable = false
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover services");
        }

        return services;
    }

    public async Task<string> GetAggregatedAsyncApiAsync(CancellationToken cancellationToken = default)
    {
        var aggregatedSpec = new JsonObject
        {
            ["asyncapi"] = "2.6.0",
            ["info"] = new JsonObject
            {
                ["title"] = "BookWorm Aggregated AsyncAPI",
                ["version"] = "1.0.0",
                ["description"] = "Aggregated AsyncAPI specifications from all BookWorm services"
            },
            ["channels"] = new JsonObject(),
            ["components"] = new JsonObject
            {
                ["schemas"] = new JsonObject(),
                ["messages"] = new JsonObject()
            }
        };

        var discoveredServices = await GetDiscoveredServicesAsync(cancellationToken);
        
        foreach (var service in discoveredServices.Where(s => s.IsAvailable))
        {
            try
            {
                var serviceSpec = await GetServiceAsyncApiAsync(service.AsyncApiUrl, cancellationToken);
                if (serviceSpec is not null)
                {
                    MergeAsyncApiSpec(aggregatedSpec, serviceSpec, service.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch AsyncAPI from service {ServiceName} at {Url}", 
                    service.Name, service.AsyncApiUrl);
            }
        }

        return aggregatedSpec.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<bool> CheckServiceAvailabilityAsync(string baseUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout
            
            var response = await _httpClient.GetAsync($"{baseUrl}/health", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<JsonObject?> GetServiceAsyncApiAsync(string asyncApiUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 second timeout
            
            var response = await _httpClient.GetStringAsync(asyncApiUrl, cts.Token);
            return JsonNode.Parse(response)?.AsObject();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to fetch AsyncAPI from {Url}", asyncApiUrl);
            return null;
        }
    }

    private static void MergeAsyncApiSpec(JsonObject target, JsonObject source, string servicePrefix)
    {
        // Merge channels with service prefix
        if (source["channels"]?.AsObject() is { } sourceChannels)
        {
            var targetChannels = target["channels"]!.AsObject();
            foreach (var channel in sourceChannels)
            {
                var prefixedChannelName = $"{servicePrefix}.{channel.Key}";
                targetChannels[prefixedChannelName] = channel.Value?.DeepClone();
            }
        }

        // Merge schemas with service prefix
        if (source["components"]?["schemas"]?.AsObject() is { } sourceSchemas)
        {
            var targetSchemas = target["components"]!["schemas"]!.AsObject();
            foreach (var schema in sourceSchemas)
            {
                var prefixedSchemaName = $"{servicePrefix}.{schema.Key}";
                targetSchemas[prefixedSchemaName] = schema.Value?.DeepClone();
            }
        }

        // Merge messages with service prefix
        if (source["components"]?["messages"]?.AsObject() is { } sourceMessages)
        {
            var targetMessages = target["components"]!["messages"]!.AsObject();
            foreach (var message in sourceMessages)
            {
                var prefixedMessageName = $"{servicePrefix}.{message.Key}";
                targetMessages[prefixedMessageName] = message.Value?.DeepClone();
            }
        }
    }
}