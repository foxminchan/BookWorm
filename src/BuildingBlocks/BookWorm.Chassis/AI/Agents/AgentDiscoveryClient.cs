using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Agents;

internal abstract class AgentDiscoveryClient(
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

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var agents =
            await JsonSerializer.DeserializeAsync<List<AgentDiscoveryCard>>(
                stream,
                AgentDiscoveryCardSerializationContext.Default.Options,
                cancellationToken
            ) ?? [];

        logger.LogInformation("Retrieved {AgentCount} agents from the API", agents.Count);

        return agents;
    }

    public sealed record AgentDiscoveryCard(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("description")] string? Description
    );
}

[JsonSerializable(typeof(AgentDiscoveryClient.AgentDiscoveryCard))]
[JsonSerializable(typeof(List<AgentDiscoveryClient.AgentDiscoveryCard>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class AgentDiscoveryCardSerializationContext : JsonSerializerContext;

public static class AgentDiscoveryClientExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers an <see cref="AgentDiscoveryClient" /> with the dependency injection container
        ///     and configures the underlying <see cref="System.Net.Http.HttpClient" /> with the specified base address.
        /// </summary>
        /// <param name="baseAddress">
        ///     The base URI of the agent discovery API endpoint.
        ///     Must be a valid absolute URI string (e.g., <c>https://my-agent-service/</c>).
        /// </param>
        public void AddAgentDiscoveryClient(
            [StringSyntax(StringSyntaxAttribute.Uri)] string baseAddress
        )
        {
            services.AddHttpClient<AgentDiscoveryClient>(client =>
            {
                client.BaseAddress = new(baseAddress);
            });
        }
    }
}
