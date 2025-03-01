using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Finance.Feature;

public sealed class GetOrderStateEndpoint : IEndpoint<Ok<string>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/order-state-machine", async (ISender sender) => await HandleAsync(sender))
            .Produces<string>()
            .WithOpenApi()
            .WithTags(nameof(OrderState))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
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
