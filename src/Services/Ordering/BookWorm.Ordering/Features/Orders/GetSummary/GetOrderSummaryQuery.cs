using BookWorm.Chassis.Utilities.Guards;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Ordering.Features.Orders.GetSummary;

public sealed record GetOrderSummaryQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] OrderId Id,
    [property: Description("Optional stream version for point-in-time replay")]
        long? Version = null,
    [property: Description("Optional UTC timestamp for point-in-time replay")]
        DateTimeOffset? At = null
) : IQuery<OrderSummaryDto>;

internal sealed class GetOrderSummaryHandler(IQuerySession querySession, IFusionCache cache)
    : IQueryHandler<GetOrderSummaryQuery, OrderSummaryDto>
{
    private static readonly FusionCacheEntryOptions _immutableCacheOptions = new()
    {
        Duration = TimeSpan.FromHours(1),
        DistributedCacheDuration = TimeSpan.FromHours(1),
    };

    public async ValueTask<OrderSummaryDto> Handle(
        GetOrderSummaryQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderId = (Guid)request.Id;

        if (request.Version is { } requestedVersion && request.At is null)
        {
            return await cache.GetOrSetAsync(
                OrderingCacheKeys.OrderSummaryVersion(orderId, requestedVersion),
                async ct => await LoadSummaryAsync(orderId, requestedVersion, null, ct),
                _immutableCacheOptions,
                cancellationToken
            );
        }

        if (request.Version is null && request.At is null)
        {
            var key = OrderingCacheKeys.OrderSummaryTerminal(orderId);
            var cached = await cache.TryGetAsync<OrderSummaryDto>(key, token: cancellationToken);
            if (cached.HasValue && cached.Value is not null)
            {
                return cached.Value;
            }

            var dto = await LoadSummaryAsync(orderId, null, null, cancellationToken);

            if (dto.Status is Status.Completed or Status.Cancelled)
            {
                await cache.SetAsync(key, dto, _immutableCacheOptions, cancellationToken);
            }

            return dto;
        }

        return await LoadSummaryAsync(orderId, request.Version, request.At, cancellationToken);
    }

    private async Task<OrderSummaryDto> LoadSummaryAsync(
        Guid orderId,
        long? requestedVersion,
        DateTimeOffset? at,
        CancellationToken cancellationToken
    )
    {
        var summary = await querySession.Events.AggregateStreamAsync<OrderSummary>(
            orderId,
            requestedVersion ?? 0,
            at,
            token: cancellationToken
        );

        Guard.Against.NotFound(summary, orderId);

        if (requestedVersion is { } explicitVersion)
        {
            return new(summary.Id, summary.Status, summary.TotalPrice, explicitVersion);
        }

        var streamState = await querySession.Events.FetchStreamStateAsync(
            orderId,
            cancellationToken
        );

        var version = streamState?.Version ?? 0;

        return new(summary.Id, summary.Status, summary.TotalPrice, version);
    }
}
