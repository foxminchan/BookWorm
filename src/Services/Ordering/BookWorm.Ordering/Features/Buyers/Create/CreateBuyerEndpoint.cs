using BookWorm.Ordering.Features.Buyers.Get;
using Mediator;

namespace BookWorm.Ordering.Features.Buyers.Create;

public sealed class CreateBuyerEndpoint
    : IEndpoint<Created<Guid>, CreateBuyerCommand, ISender, LinkGenerator>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/buyers",
                async (CreateBuyerCommand command, ISender sender, LinkGenerator linker) =>
                    await HandleAsync(command, sender, linker)
            )
            .ProducesPost<Guid>()
            .WithTags(nameof(Buyer))
            .WithName(nameof(CreateBuyerEndpoint))
            .WithSummary("Create Buyer")
            .WithDescription("Create a new buyer in the system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Created<Guid>> HandleAsync(
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
