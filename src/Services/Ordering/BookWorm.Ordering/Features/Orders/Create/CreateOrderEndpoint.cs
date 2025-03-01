namespace BookWorm.Ordering.Features.Orders.Create;

public sealed class CreateOrderEndpoint : IEndpoint<Created<Guid>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/orders",
                async ([FromIdempotencyHeader] string key, ISender sender) =>
                    await HandleAsync(sender)
            )
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithIdempotency()
            .WithOpenApi()
            .WithTags(nameof(Order))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await request.Send(new CreateOrderCommand(), cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithResource(nameof(Orders)).WithId(result).Build(),
            result
        );
    }
}
