using BookWorm.Chassis.Security.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Dashboard;

internal sealed class GetDashboardEndpoint : IEndpoint<Ok<DashboardDto>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/dashboard",
                async (ISender sender, CancellationToken ct) => await HandleAsync(sender, ct)
            )
            .ProducesGet<DashboardDto>(true)
            .WithTags("Dashboard")
            .WithName(nameof(GetDashboardEndpoint))
            .WithSummary("Get backoffice dashboard")
            .WithDescription(
                "Get store-wide totals, seven UTC daily buckets, categories, and the five newest orders"
            )
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequireAuthorization(policy =>
                policy.RequireScope($"{Services.Catalog}_{Authorization.Actions.Read}")
            )
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<DashboardDto>> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        return TypedResults.Ok(await sender.Send(new GetDashboardQuery(), cancellationToken));
    }
}
