using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Extensions;

public static class ActorFrameworkWebApplicationExtensions
{
    public static void MapAgentDiscovery(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string path
    )
    {
        var registeredAIAgents = endpoints.ServiceProvider.GetKeyedServices<AIAgent>(
            KeyedService.AnyKey
        );

        var routeGroup = endpoints.MapGroup(path);

        routeGroup
            .MapGet(
                "/",
                Ok<List<AgentDiscoveryCard>> () =>
                {
                    var results = registeredAIAgents
                        .Select(result => new AgentDiscoveryCard
                        {
                            Id = result.Id,
                            Name = result.Name ?? "Unnamed Agent",
                            Description = result.Description,
                        })
                        .ToList();

                    return TypedResults.Ok(results);
                }
            )
            .WithName("GetAgents");
    }

    private sealed record AgentDiscoveryCard
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; init; }
    }
}
