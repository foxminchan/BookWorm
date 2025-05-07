namespace BookWorm.Catalog.Features.Publishers.List;

public sealed class ListPublishersEndpoint : IEndpoint<Ok<IReadOnlyList<PublisherDto>>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/publishers", async (ISender sender) => await HandleAsync(sender))
            .ProducesGet<IReadOnlyList<PublisherDto>>()
            .WithTags(nameof(Publisher))
            .WithName(nameof(ListPublishersEndpoint))
            .WithSummary("List Publishers")
            .WithDescription("Lists all publishers")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<IReadOnlyList<PublisherDto>>> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new ListPublishersQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
