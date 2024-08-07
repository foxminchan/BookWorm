﻿using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.Features.Orders.List;

public sealed class ListOrdersEndpoint : IEndpoint<Ok<List<OrderDto>>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (ISender sender) => await HandleAsync(sender))
            .Produces<Ok<IEnumerable<OrderDto>>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Order))
            .WithName("List Orders")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<List<OrderDto>>> HandleAsync(ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListOrdersQuery(), cancellationToken);

        return TypedResults.Ok(result.Value.ToOrderDtos());
    }
}
