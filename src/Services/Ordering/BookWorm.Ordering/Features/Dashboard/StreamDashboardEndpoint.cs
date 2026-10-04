using System.Runtime.CompilerServices;
using BookWorm.Chassis.Security.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Dashboard;

internal sealed class StreamDashboardEndpoint
    : IEndpoint<ServerSentEventsResult<DashboardDto>, ISender, ClaimsPrincipal>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/dashboard/stream",
                async (ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
                    await HandleAsync(sender, user, ct)
            )
            .WithTags("Dashboard")
            .WithName(nameof(StreamDashboardEndpoint))
            .WithSummary("Stream backoffice dashboard")
            .WithDescription(
                "Send cached dashboard snapshots every 15 seconds; reconnect to resynchronize"
            )
            .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequireAuthorization(policy =>
                policy.RequireScope($"{Services.Catalog}_{Authorization.Actions.Read}")
            )
            .RequirePerUserRateLimit();
    }

    public async Task<ServerSentEventsResult<DashboardDto>> HandleAsync(
        ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    )
    {
        var initial = await sender.Send(new GetDashboardQuery(), cancellationToken);
        var expiresAt = long.TryParse(user.FindFirst("exp")?.Value, out var expiry)
            ? DateTimeOffset.FromUnixTimeSeconds(expiry)
            : DateTimeOffset.UtcNow.AddMinutes(5);
        return TypedResults.ServerSentEvents(
            StreamAsync(sender, initial, cancellationToken, expiresAt),
            "dashboard"
        );
    }

    internal static async IAsyncEnumerable<DashboardDto> StreamAsync(
        ISender sender,
        DashboardDto initial,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        DateTimeOffset? expiresAt = null
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        yield return initial;
        var deadline = DateTimeOffset.UtcNow.AddMinutes(5);
        if (expiresAt < deadline)
        {
            deadline = expiresAt.Value;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (DateTimeOffset.UtcNow >= deadline)
            {
                yield break;
            }

            yield return await sender.Send(new GetDashboardQuery(), cancellationToken);
        }
    }
}
