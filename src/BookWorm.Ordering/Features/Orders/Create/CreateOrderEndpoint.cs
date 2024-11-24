namespace BookWorm.Ordering.Features.Orders.Create;

public sealed class CreateOrderEndpoint : IEndpoint<Created<Guid>, CreateOrderCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/orders",
                async (
                    [FromIdempotencyHeader] string key,
                    CreateOrderCommand command,
                    ISender sender
                ) => await HandleAsync(command, sender)
            )
            .AddEndpointFilter<IdempotencyFilter>()
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Order))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateOrderCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder()
                .WithVersion()
                .WithResource(nameof(Orders))
                .WithId(result.Value)
                .Build(),
            result.Value
        );
    }
}
