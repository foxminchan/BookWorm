using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.Features.Orders.Get;

public sealed class GetOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}",
                async (Guid orderId, ISender sender) => await HandleAsync(orderId, sender))
            .Produces<Ok<OrderDetailDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Order))
            .WithName("Get Order")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<OrderDetailDto>> HandleAsync(Guid orderId, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetOrderQuery(orderId), cancellationToken);

        return TypedResults.Ok(result.Value);
    }
}
