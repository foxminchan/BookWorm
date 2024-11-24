namespace BookWorm.Rating.Features.List;

public sealed class ListFeedbackEndpoint
    : IEndpoint<Ok<PagedItems<Feedback>>, ListFeedbackQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/feedbacks",
                async ([AsParameters] ListFeedbackQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<Ok<PagedItems<Feedback>>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Feedback))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<PagedItems<Feedback>>> HandleAsync(
        ListFeedbackQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var feedbacks = await sender.Send(query, cancellationToken);

        var response = new PagedItems<Feedback>(feedbacks.PagedInfo, feedbacks.Value.ToList());

        return TypedResults.Ok(response);
    }
}
