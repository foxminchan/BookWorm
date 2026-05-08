using BookWorm.Catalog.Domain.EventHandlers;
using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.UnitTests.Domain.EventHandlers;

public sealed class BookChangedEventHandlerTests
{
    private readonly Mock<IFusionCache> _cacheMock = new();
    private readonly BookChangedEventHandler _handler;

    public BookChangedEventHandlerTests()
    {
        _handler = new(_cacheMock.Object);
    }

    [Test]
    public async Task GivenBookChangedEvent_WhenHandling_ThenShouldRemoveCacheEntry()
    {
        // Arrange
        const string cacheKey = "catalog:books:list";
        var @event = new BookChangedEvent(cacheKey);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    cacheKey,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDifferentCacheKeys_WhenHandling_ThenShouldRemoveCorrectKey()
    {
        // Arrange
        const string cacheKey = "catalog:books:detail:abc123";
        var @event = new BookChangedEvent(cacheKey);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    "catalog:books:detail:abc123",
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
