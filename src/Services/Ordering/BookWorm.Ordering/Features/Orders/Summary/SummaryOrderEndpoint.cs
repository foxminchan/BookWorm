using System.ComponentModel;
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
                    [Description("The order id")] Guid id
                ) => querySession.Json.WriteById<OrderSummaryInfo>(id, context)
            )
            .Produces<OrderSummaryInfo>()
            .WithOpenApi()
            .WithTags(nameof(Order))
            .MapToApiVersion(new(1, 0));
    }
}
