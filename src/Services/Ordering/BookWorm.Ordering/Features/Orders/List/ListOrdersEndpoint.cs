﻿using BookWorm.SharedKernel.Results;

namespace BookWorm.Ordering.Features.Orders.List;

public sealed class ListOrdersEndpoint
    : IEndpoint<Ok<PagedResult<OrderDto>>, ListOrdersQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders",
                async ([AsParameters] ListOrdersQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<PagedResult<OrderDto>>()
            .ProducesValidationProblem()
            .WithTags(nameof(Order))
            .WithName(nameof(ListOrdersEndpoint))
            .WithSummary("List Orders")
            .WithDescription("List orders with pagination and filtering")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<PagedResult<OrderDto>>> HandleAsync(
        ListOrdersQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
