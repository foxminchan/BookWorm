using System.Net.Mime;

namespace BookWorm.Chat.Features.Visualizer;

public sealed class VisualizerWorkflowEndpoint
    : IEndpoint<Ok<string>, VisualizerWorkflowQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/visualizer/workflow",
                async ([AsParameters] VisualizerWorkflowQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<string>(contentType: MediaTypeNames.Text.Plain)
            .WithTags(nameof(Chat))
            .WithName(nameof(VisualizerWorkflowEndpoint))
            .WithSummary("Visualizer Workflow")
            .WithDescription("Get the workflow for the visualizer")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<string>> HandleAsync(
        VisualizerWorkflowQuery query,
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await request.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}
