using BookWorm.Rating.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Rating.Domain.EventHandlers;

internal sealed class FeedbackCacheInvalidationHandler(IFusionCache cache)
    : INotificationHandler<FeedbackCreatedEvent>,
        INotificationHandler<FeedbackDeletedEvent>,
        INotificationHandler<FeedbackRevertedEvent>
{
    public async ValueTask Handle(
        FeedbackCreatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveByTagAsync(
            RatingCacheKeys.FeedbackTag(notification.BookId),
            token: cancellationToken
        );

        var summaryKey = RatingCacheKeys.Summary(notification.BookId);
        var cachedSummary = await cache.TryGetAsync<SummarizeResult>(
            summaryKey,
            token: cancellationToken
        );
        if (cachedSummary is { HasValue: true, Value.ReviewCount: 0 })
        {
            await cache.RemoveAsync(summaryKey, token: cancellationToken);
        }
    }

    public async ValueTask Handle(
        FeedbackDeletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateAllAsync(notification.BookId, cancellationToken);
    }

    public async ValueTask Handle(
        FeedbackRevertedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateAllAsync(notification.BookId, cancellationToken);
    }

    private async Task InvalidateAllAsync(Guid bookId, CancellationToken cancellationToken)
    {
        await cache.RemoveByTagAsync(RatingCacheKeys.FeedbackTag(bookId), token: cancellationToken);
        await cache.RemoveAsync(RatingCacheKeys.Summary(bookId), token: cancellationToken);
    }
}
