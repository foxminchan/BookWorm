using BookWorm.Constants.Core;
using BookWorm.Rating.Infrastructure.Summarizer;
using Mediator;

namespace BookWorm.Rating.Features.Summarize;

internal sealed class SummarizeFeedbackEndpoint : IEndpoint<Ok<SummarizeResult>, BookId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/{id}/summarize",
                async (
                    [Description("The unique identifier of the book to be summarized")] BookId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesGet<SummarizeResult>(hasNotFound: true)
            .WithTags(nameof(Feedback))
            .WithName(nameof(SummarizeFeedbackEndpoint))
            .WithSummary("Summarize Feedback")
            .WithDescription("Summarize the feedback for a book by its ID")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<SummarizeResult>> HandleAsync(
        BookId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new SummarizeFeedbackQuery((Guid)id), cancellationToken);

        return TypedResults.Ok(result);
    }
}
