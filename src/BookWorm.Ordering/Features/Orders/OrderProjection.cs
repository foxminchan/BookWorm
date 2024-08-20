using BookWorm.Ordering.Domain.OrderAggregate.Events;

namespace BookWorm.Ordering.Features.Orders;

public sealed class OrderStateEndpoint : IEndpoint<Ok, Guid, HttpContext, IQuerySession>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/state",
                async (HttpContext context, IQuerySession querySession, Guid id) =>
                    await HandleAsync(id, context, querySession))
            .Produces<Ok>()
            .WithTags(nameof(Order))
            .WithName("Get Order State")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok> HandleAsync(Guid id, HttpContext context, IQuerySession querySession,
        CancellationToken cancellationToken = default)
    {
        await querySession.Json.WriteById<OrderQuery>(id, context);
        return TypedResults.Ok();
    }
}

public sealed record OrderQuery(Guid Id, Status Status);

public sealed class OrderProjection : MultiStreamProjection<OrderQuery, Guid>
{
    public OrderProjection()
    {
        Identity<OrderCreatedEvent>(e => e.Id);
        Identity<OrderCompletedEvent>(e => e.Id);
        Identity<OrderCancelledEvent>(e => e.Id);
    }

    public OrderQuery Apply(OrderQuery query, OrderCreatedEvent @event)
    {
        return query with { Status = Status.Pending };
    }

    public OrderQuery Apply(OrderQuery query, OrderCompletedEvent @event)
    {
        return query with { Status = Status.Completed };
    }

    public OrderQuery Apply(OrderQuery query, OrderCancelledEvent @event)
    {
        return query with { Status = Status.Canceled };
    }
}
