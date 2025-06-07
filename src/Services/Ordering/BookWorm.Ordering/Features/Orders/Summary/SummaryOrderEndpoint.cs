using Marten.AspNetCore;

namespace BookWorm.Ordering.Features.Orders.Summary;

public sealed class SummaryOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{id:guid}/summary",
                (
                    HttpContext context,
                    IQuerySession querySession,
                    [Description("The unique identifier of the order to be retrieved")] Guid id
                ) => querySession.Json.WriteById<OrderSummaryView>(id, context)
            )
            .ProducesGet<OrderSummaryView>()
            .WithTags(nameof(Order))
            .WithName(nameof(SummaryOrderEndpoint))
            .WithSummary("Get Order Summary")
            .WithDescription("Get an order summary if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }
}
