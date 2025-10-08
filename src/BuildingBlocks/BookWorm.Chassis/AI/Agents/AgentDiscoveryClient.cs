using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Agents;

public abstract class AgentDiscoveryClient(
    HttpClient httpClient,
    ILogger<AgentDiscoveryClient> logger
)
{
    public async Task<List<AgentDiscoveryCard>> GetAgentsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync(
            new Uri("/agents", UriKind.Relative),
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var agents =
            JsonSerializer.Deserialize<List<AgentDiscoveryCard>>(
                json,
                AgentDiscoveryCardSerializationContext.Default.Options
            ) ?? [];

        logger.LogInformation("Retrieved {AgentCount} agents from the API", agents.Count);

        return agents;
    }

    public sealed class AgentDiscoveryCard
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}

[JsonSerializable(typeof(AgentDiscoveryClient.AgentDiscoveryCard))]
[JsonSerializable(typeof(List<AgentDiscoveryClient.AgentDiscoveryCard>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class AgentDiscoveryCardSerializationContext : JsonSerializerContext;
