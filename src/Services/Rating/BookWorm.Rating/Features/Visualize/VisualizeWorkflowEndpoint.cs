using System.Net.Mime;
using BookWorm.Constants.Core;

namespace BookWorm.Rating.Features.Visualize;

public sealed class VisualizeWorkflowEndpoint
    : IEndpoint<Ok<string>, VisualizeWorkflowQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/visualize",
                async ([AsParameters] VisualizeWorkflowQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<string>(contentType: MediaTypeNames.Text.Plain)
            .WithTags(nameof(Rating))
            .WithName(nameof(VisualizeWorkflowEndpoint))
            .WithSummary("Visualizer Workflow")
            .WithDescription("Get the workflow for the visualizer")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<string>> HandleAsync(
        VisualizeWorkflowQuery query,
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await request.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}
