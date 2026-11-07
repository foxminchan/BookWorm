using BookWorm.Rating.Features.Summarize;
using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.Rating.Infrastructure.Summarizer.Models;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Rating.UnitTests.Features.Summarize;

public sealed class SummarizeFeedbackQueryTests
{
    private static readonly TimeSpan _summaryDuration = TimeSpan.FromHours(6);

    private static string CacheKey(Guid bookId)
    {
        return $"rating:summary:v1:{bookId}";
    }

    private static FusionCache CreateCache()
    {
        return new(new FusionCacheOptions());
    }

    [Test]
    public async Task GivenCacheMiss_WhenHandlingQuery_ThenShouldSummarizeAndCacheResult()
    {
        var bookId = Guid.CreateVersion7();
        var expected = new SummarizeResult("Summary", RatingClassification.Good, 4.5, 2);
        var summarizer = new Mock<ISummarizer>();
        summarizer
            .Setup(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        using var cache = CreateCache();
        var handler = new SummarizeFeedbackHandler(summarizer.Object, cache);

        var result = await handler.Handle(new(bookId), CancellationToken.None);

        result.ShouldBe(expected);
        (await cache.TryGetAsync<SummarizeResult>(CacheKey(bookId))).Value.ShouldBe(expected);
        summarizer.Verify(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCachedSummary_WhenHandlingQueryAgain_ThenShouldNotCallSummarizer()
    {
        var bookId = Guid.CreateVersion7();
        var expected = new SummarizeResult("Summary", RatingClassification.Good, 4.5, 2);
        var summarizer = new Mock<ISummarizer>();
        summarizer
            .Setup(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        using var cache = CreateCache();
        var handler = new SummarizeFeedbackHandler(summarizer.Object, cache);

        await handler.Handle(new(bookId), CancellationToken.None);
        var result = await handler.Handle(new(bookId), CancellationToken.None);

        result.ShouldBe(expected);
        summarizer.Verify(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenDifferentBooks_WhenHandlingQuery_ThenShouldCacheSeparately()
    {
        var firstBookId = Guid.CreateVersion7();
        var secondBookId = Guid.CreateVersion7();
        var summarizer = new Mock<ISummarizer>();
        summarizer
            .Setup(s => s.SummarizeAsync(firstBookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SummarizeResult("First"));
        summarizer
            .Setup(s => s.SummarizeAsync(secondBookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SummarizeResult("Second"));
        using var cache = CreateCache();
        var handler = new SummarizeFeedbackHandler(summarizer.Object, cache);

        var first = await handler.Handle(new(firstBookId), CancellationToken.None);
        var second = await handler.Handle(new(secondBookId), CancellationToken.None);

        first.Summary.ShouldBe("First");
        second.Summary.ShouldBe("Second");
    }

    [Test]
    public async Task GivenSummarizerFailure_WhenHandlingQuery_ThenShouldPropagateAndNotCache()
    {
        var bookId = Guid.CreateVersion7();
        var summarizer = new Mock<ISummarizer>();
        summarizer
            .Setup(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Workflow failed."));
        using var cache = CreateCache();
        var handler = new SummarizeFeedbackHandler(summarizer.Object, cache);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.Handle(new(bookId), CancellationToken.None).AsTask()
        );

        (await cache.TryGetAsync<SummarizeResult>(CacheKey(bookId))).HasValue.ShouldBeFalse();
    }

    [Test]
    public async Task GivenCacheMiss_WhenHandlingQuery_ThenShouldUseSummaryTtl()
    {
        var bookId = Guid.CreateVersion7();
        var expected = new SummarizeResult("Summary");
        var cache = new Mock<IFusionCache>();
        cache
            .Setup(c =>
                c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<SummarizeResult>,
                            CancellationToken,
                            Task<SummarizeResult>
                        >
                    >(),
                    It.IsAny<MaybeValue<SummarizeResult>>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expected);
        var handler = new SummarizeFeedbackHandler(Mock.Of<ISummarizer>(), cache.Object);

        var result = await handler.Handle(new(bookId), CancellationToken.None);

        result.ShouldBe(expected);
        cache.Verify(
            c =>
                c.GetOrSetAsync(
                    CacheKey(bookId),
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<SummarizeResult>,
                            CancellationToken,
                            Task<SummarizeResult>
                        >
                    >(),
                    It.IsAny<MaybeValue<SummarizeResult>>(),
                    It.Is<FusionCacheEntryOptions?>(options =>
                        options != null
                        && options.Duration == _summaryDuration
                        && options.DistributedCacheDuration == _summaryDuration
                    ),
                    It.IsAny<IEnumerable<string>?>(),
                    CancellationToken.None
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingQuery_ThenShouldPropagateIt()
    {
        var bookId = Guid.CreateVersion7();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var summarizer = new Mock<ISummarizer>();
        summarizer
            .Setup(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        using var cache = CreateCache();
        var handler = new SummarizeFeedbackHandler(summarizer.Object, cache);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            handler.Handle(new(bookId), cancellation.Token).AsTask()
        );
    }
}
