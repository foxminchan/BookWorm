using BookWorm.Rating.Domain.EventHandlers;
using BookWorm.Rating.Domain.Events;
using BookWorm.Rating.Infrastructure.Summarizer.Models;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Rating.UnitTests.Domain.EventHandlers;

public sealed class FeedbackCacheInvalidationHandlerTests : IDisposable
{
    private readonly FusionCache _cache;
    private readonly FeedbackCacheInvalidationHandler _handler;

    public FeedbackCacheInvalidationHandlerTests()
    {
        _cache = new(new FusionCacheOptions());
        _handler = new(_cache);
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task GivenFeedbackCreatedWithZeroReviewSummary_WhenHandling_ThenShouldEvictListAndSummary()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var listKey = RatingCacheKeys.FeedbackList(bookId, 1, 10, null, false);
        var summaryKey = RatingCacheKeys.Summary(bookId);

        await _cache.SetAsync(listKey, "cached-page", tags: [RatingCacheKeys.FeedbackTag(bookId)]);
        await _cache.SetAsync(summaryKey, new SummarizeResult("No feedback found for this book."));

        // Act
        await _handler.Handle(
            new FeedbackCreatedEvent(bookId, 5, Guid.CreateVersion7()),
            CancellationToken.None
        );

        // Assert
        (await _cache.TryGetAsync<string>(listKey)).HasValue.ShouldBeFalse();
        (await _cache.TryGetAsync<SummarizeResult>(summaryKey)).HasValue.ShouldBeFalse();
    }

    [Test]
    public async Task GivenFeedbackCreatedWithNonEmptySummary_WhenHandling_ThenShouldEvictListAndPreserveSummary()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var listKey = RatingCacheKeys.FeedbackList(bookId, 1, 10, null, false);
        var summaryKey = RatingCacheKeys.Summary(bookId);

        await _cache.SetAsync(listKey, "cached-page", tags: [RatingCacheKeys.FeedbackTag(bookId)]);
        await _cache.SetAsync(
            summaryKey,
            new SummarizeResult("Existing summary", RatingClassification.Good, 4.5, 3)
        );

        // Act
        await _handler.Handle(
            new FeedbackCreatedEvent(bookId, 5, Guid.CreateVersion7()),
            CancellationToken.None
        );

        // Assert
        (await _cache.TryGetAsync<string>(listKey)).HasValue.ShouldBeFalse();
        (await _cache.TryGetAsync<SummarizeResult>(summaryKey)).HasValue.ShouldBeTrue();
    }

    [Test]
    public async Task GivenFeedbackDeletedEvent_WhenHandling_ThenShouldEvictListAndSummary()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var listKey = RatingCacheKeys.FeedbackList(bookId, 1, 10, null, false);
        var summaryKey = RatingCacheKeys.Summary(bookId);

        await _cache.SetAsync(listKey, "cached-page", tags: [RatingCacheKeys.FeedbackTag(bookId)]);
        await _cache.SetAsync(
            summaryKey,
            new SummarizeResult("Summary", RatingClassification.Good, 4.5, 2)
        );

        // Act
        await _handler.Handle(
            new FeedbackDeletedEvent(bookId, 4, Guid.CreateVersion7()),
            CancellationToken.None
        );

        // Assert
        (await _cache.TryGetAsync<string>(listKey)).HasValue.ShouldBeFalse();
        (await _cache.TryGetAsync<SummarizeResult>(summaryKey)).HasValue.ShouldBeFalse();
    }

    [Test]
    public async Task GivenFeedbackRevertedEvent_WhenHandling_ThenShouldEvictListAndSummary()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var listKey = RatingCacheKeys.FeedbackList(bookId, 1, 10, null, false);
        var summaryKey = RatingCacheKeys.Summary(bookId);

        await _cache.SetAsync(listKey, "cached-page", tags: [RatingCacheKeys.FeedbackTag(bookId)]);
        await _cache.SetAsync(
            summaryKey,
            new SummarizeResult("Summary", RatingClassification.Good, 4.5, 2)
        );

        // Act
        await _handler.Handle(new FeedbackRevertedEvent(bookId), CancellationToken.None);

        // Assert
        (await _cache.TryGetAsync<string>(listKey)).HasValue.ShouldBeFalse();
        (await _cache.TryGetAsync<SummarizeResult>(summaryKey)).HasValue.ShouldBeFalse();
    }
}
