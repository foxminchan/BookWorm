using BookWorm.Constants.Core;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Finance.Feature;

public sealed class GetOrderStateEndpoint : IEndpoint<Ok<string>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/order-state-machine", async (ISender sender) => await HandleAsync(sender))
            .ProducesGet<string>()
            .WithTags(nameof(OrderState))
            .WithName(nameof(GetOrderStateEndpoint))
            .WithSummary("Get Order State Machine")
            .WithDescription("Get the order state machine definition")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<string>> HandleAsync(
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await request.Send(new GetOrderStateQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
