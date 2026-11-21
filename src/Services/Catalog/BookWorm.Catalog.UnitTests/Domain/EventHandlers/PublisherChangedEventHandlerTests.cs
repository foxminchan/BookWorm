using BookWorm.Catalog.Domain.EventHandlers;
using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.UnitTests.Domain.EventHandlers;

public sealed class PublisherChangedEventHandlerTests
{
    private readonly Mock<IFusionCache> _cacheMock = new();
    private readonly PublisherChangedEventHandler _handler;

    public PublisherChangedEventHandlerTests()
    {
        _handler = new(_cacheMock.Object);
    }

    [Test]
    public async Task GivenPublisherCreatedEvent_WhenHandling_ThenShouldRemovePublishersKey()
    {
        await _handler.Handle(new PublisherCreatedEvent(), CancellationToken.None);

        _cacheMock.Verify(
            c =>
                c.RemoveAsync(
                    CatalogCacheKeys.Publishers,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            c =>
                c.RemoveByTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenPublisherUpdatedEvent_WhenHandling_ThenShouldEvictPublishersAndBooks()
    {
        await _handler.Handle(new PublisherUpdatedEvent(), CancellationToken.None);

        VerifyEvictedWithBooks();
    }

    [Test]
    public async Task GivenPublisherDeletedEvent_WhenHandling_ThenShouldEvictPublishersAndBooks()
    {
        await _handler.Handle(new PublisherDeletedEvent(), CancellationToken.None);

        VerifyEvictedWithBooks();
    }

    private void VerifyEvictedWithBooks()
    {
        _cacheMock.Verify(
            c =>
                c.RemoveAsync(
                    CatalogCacheKeys.Publishers,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            c =>
                c.RemoveByTagAsync(
                    CatalogCacheKeys.BookTag,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            c =>
                c.RemoveByTagAsync(
                    CatalogCacheKeys.BookListTag,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
