﻿namespace BookWorm.Ordering.Features.Buyers.UpdateAddress;

public sealed class UpdateAddressEndpoint : IEndpoint<Ok<BuyerDto>, UpdateAddressCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/buyers/address",
                async (UpdateAddressCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPatch<BuyerDto>()
            .WithTags(nameof(Buyer))
            .WithName(nameof(UpdateAddressEndpoint))
            .WithSummary("Update Buyer Address")
            .WithDescription("Update the current buyer's address")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.User)
            .RequirePerUserRateLimit();
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
