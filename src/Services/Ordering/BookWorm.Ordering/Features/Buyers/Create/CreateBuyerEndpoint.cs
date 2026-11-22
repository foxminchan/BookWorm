using BookWorm.Ordering.Features.Buyers.Get;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Features.Buyers.Create;

internal sealed class CreateBuyerEndpoint
    : IEndpoint<Created<BuyerId>, CreateBuyerCommand, ISender, LinkGenerator>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/buyers",
                async (
                    [FromHeader(Name = Http.RequestIdHeader)]
                    [Description("The idempotency key of the buyer to be created")]
                        string key,
                    CreateBuyerCommand command,
                    ISender sender,
                    LinkGenerator linker
                ) => await HandleAsync(command, sender, linker)
            )
            .ProducesPost<BuyerId>()
            .WithIdempotency()
            .WithTags(nameof(Buyer))
            .WithName(nameof(CreateBuyerEndpoint))
            .WithSummary("Create Buyer")
            .WithDescription("Create a new buyer in the system")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Created<BuyerId>> HandleAsync(
        CreateBuyerCommand command,
        ISender sender,
        LinkGenerator linker,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        var path = linker.GetPathByName(nameof(GetBuyerEndpoint), new { id = result });

        return TypedResults.Created(path, result);
    }
}
