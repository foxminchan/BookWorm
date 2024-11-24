using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.Features.Buyers.Create;

public sealed class CreateBuyerEndpoint : IEndpoint<Created<Guid>, CreateBuyerCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/buyers",
                async (CreateBuyerCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Buyer))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateBuyerCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder()
                .WithVersion()
                .WithResource(nameof(Buyers))
                .WithId(result.Value)
                .Build(),
            result.Value
        );
    }
}
