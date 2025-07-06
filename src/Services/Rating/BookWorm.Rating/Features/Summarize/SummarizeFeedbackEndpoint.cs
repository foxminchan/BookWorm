namespace BookWorm.Rating.Features.Summarize;

public sealed class SummarizeFeedbackEndpoint : IEndpoint<Ok<SummarizeResult>, Guid, ISender>
{
    /// <summary>
    /// Configures an HTTP GET endpoint for summarizing feedback for a book by its unique identifier.
    /// </summary>
    /// <param name="app">The endpoint route builder used to map the endpoint.</param>
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

    /// <summary>
    /// Handles a request to summarize feedback for a specific book by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book to summarize feedback for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An HTTP 200 OK response containing the summarized feedback result.</returns>
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
