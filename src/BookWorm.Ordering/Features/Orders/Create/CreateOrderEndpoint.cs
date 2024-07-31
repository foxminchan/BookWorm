using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed record CreateOrderRequest(string? Note);

public sealed class CreateOrderEndpoint : IEndpoint<Created<Guid>, CreateOrderRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders",
                async (CreateOrderRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags(nameof(Order))
            .WithName("Create Order")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(CreateOrderRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CreateOrderCommand(request.Note), cancellationToken);

        return TypedResults.Created($"/api/v1/orders/{result.Value}", result.Value);
    }
}
