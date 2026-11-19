namespace BookWorm.Ordering.Features.Orders.GetSummary;

internal sealed class GetOrderSummaryEndpoint
    : IEndpoint<Ok<OrderSummaryDto>, GetOrderSummaryQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{id}/summary",
                async (
                    [Description("The unique identifier of the order")] OrderId id,
                    [Description("Optional stream version for point-in-time replay")] long? version,
                    [Description("Optional UTC timestamp for point-in-time replay")]
                        DateTimeOffset? at,
                    ISender sender,
                    CancellationToken cancellationToken
                ) => await HandleAsync(new(id, version, at), sender, cancellationToken)
            )
            .ProducesGet<OrderSummaryDto>(hasNotFound: true)
            .WithETag()
            .WithTags(nameof(Order))
            .WithName(nameof(GetOrderSummaryEndpoint))
            .WithSummary("Get Order Event-Sourced Summary")
            .WithDescription(
                "Reconstructs the order summary from its event stream (optionally at a specific version) and returns its stream version as an ETag"
            )
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<OrderSummaryDto>> HandleAsync(
        GetOrderSummaryQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var summary = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(summary);
    }
}
