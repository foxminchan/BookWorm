namespace BookWorm.Rating.Features.Summarize;

public sealed class SummarizeFeedbackEndpoint : IEndpoint<Ok<SummarizeResult>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/{id:guid}",
                async (
                    [Description("The unique identifier of the book to be summarized")] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken
                ) => await HandleAsync(id, sender, cancellationToken)
            )
            .ProducesGet<SummarizeResult>(hasNotFound: true)
            .WithTags(nameof(Feedback))
            .WithName(nameof(SummarizeFeedbackEndpoint))
            .WithSummary("Summarize Feedback")
            .WithDescription("Summarize the feedback for a book by its ID")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<SummarizeResult>> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new SummarizeFeedbackQuery(id), cancellationToken);

        return TypedResults.Ok(result);
    }
}
