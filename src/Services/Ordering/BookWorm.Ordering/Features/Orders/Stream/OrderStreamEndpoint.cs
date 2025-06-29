using Microsoft.AspNetCore.SignalR;

namespace BookWorm.Ordering.Features.Orders.Stream;

public sealed class OrderStreamHub : Hub;

public sealed class OrderStreamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapHub<OrderStreamHub>("/stream", options => options.AllowStatefulReconnects = true)
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }
}
