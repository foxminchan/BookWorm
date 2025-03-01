namespace BookWorm.Ordering.Features.Buyers.UpdateAddress;

public sealed class UpdateAddressEndpoint : IEndpoint<Ok<BuyerDto>, UpdateAddressCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/buyers/address",
                async (UpdateAddressCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<BuyerDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Buyer))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<BuyerDto>> HandleAsync(
        UpdateAddressCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var buyer = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(buyer);
    }
}
