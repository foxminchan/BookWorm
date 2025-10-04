using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Chassis.AI.Extensions;

public static class ActorFrameworkWebApplicationExtensions
{
    public static void MapAgentDiscovery(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string path
    )
    {
        var routeGroup = endpoints.MapGroup(path);

        routeGroup
            .MapGet(
                "/",
                async (AgentCatalog agentCatalog, CancellationToken cancellationToken) =>
                {
                    var results = new List<AgentDiscoveryCard>();
                    await foreach (
                        var result in agentCatalog
                            .GetAgentsAsync(cancellationToken)
                            .ConfigureAwait(false)
                    )
                    {
                        results.Add(
                            new()
                            {
                                Name = result.Name ?? string.Empty,
                                Description = result.Description,
                            }
                        );
                    }

                    return TypedResults.Ok(results);
                }
            )
            .WithName("GetAgents");
    }

    private sealed record AgentDiscoveryCard
    {
        public required string Name { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; init; }
    }
}
