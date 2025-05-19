using BookWorm.Constants.Core;
using BookWorm.Ordering.Features.Orders.Get;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed class CreateOrderEndpoint : IEndpoint<Created<Guid>, ISender, LinkGenerator>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/orders",
                async (
                    [FromHeader(Name = Restful.RequestIdHeader)]
                    [Description("The idempotency key of the order to be created")]
                        string key,
                    ISender sender,
                    LinkGenerator linker
                ) => await HandleAsync(sender, linker)
            )
            .ProducesPost<Guid>(false)
            .WithIdempotency()
            .WithTags(nameof(Order))
            .WithName(nameof(CreateOrderEndpoint))
            .WithSummary("Create Order")
            .WithDescription("Create a new order")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(
        ISender request,
        LinkGenerator linker,
        CancellationToken cancellationToken = default
    )
    {
        var result = await request.Send(new CreateOrderCommand(), cancellationToken);

        var path = linker.GetPathByName(nameof(GetOrderEndpoint), new { id = result });

        return TypedResults.Created(path, result);
    }
}
