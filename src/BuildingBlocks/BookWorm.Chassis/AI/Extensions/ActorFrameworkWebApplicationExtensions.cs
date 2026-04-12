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
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        ///     Maps an AI agent discovery endpoint for all keyed <see cref="AIAgent" /> registrations.
        /// </summary>
        /// <param name="path">The route prefix used for the discovery endpoint.</param>
        public void MapAgentDiscovery([StringSyntax("Route")] string path)
        {
            // Resolve all keyed AI agents from the container so they can be exposed via discovery.
            var registeredAIAgents = endpoints.ServiceProvider.GetKeyedServices<AIAgent>(
                KeyedService.AnyKey
            );

            // Group discovery routes under the provided path.
            var routeGroup = endpoints.MapGroup(path);

            routeGroup
                .MapGet(
                    "/",
                    Ok<List<AgentDiscoveryCard>> () =>
                    {
                        // Project internal agent instances into a discovery-safe response model.
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
    }

    private sealed record AgentDiscoveryCard
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; init; }
    }
}
