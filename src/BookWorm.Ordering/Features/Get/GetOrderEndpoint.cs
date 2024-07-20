using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.Features.Get;

public sealed class GetOrderEndpoint : IEndpoint<Ok<UserOrderDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}",
                async (Guid orderId, ISender sender) => await HandleAsync(orderId, sender))
            .Produces<Ok<UserOrderDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Order))
            .WithName("Get User Order")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<UserOrderDto>> HandleAsync(Guid orderId, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetOrderQuery(orderId), cancellationToken);

        return TypedResults.Ok(result.Value.ToUserOrderDto());
    }
}
