using BookWorm.Catalog.Domain.EventHandlers;
using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.UnitTests.Domain.EventHandlers;

public sealed class AuthorChangedEventHandlerTests
{
    private readonly Mock<IFusionCache> _cacheMock = new();
    private readonly AuthorChangedEventHandler _handler;

    public AuthorChangedEventHandlerTests()
    {
        _handler = new(_cacheMock.Object);
    }

    [Test]
    public async Task GivenAuthorCreatedEvent_WhenHandling_ThenShouldRemoveAuthorsKey()
    {
        await _handler.Handle(new AuthorCreatedEvent(), CancellationToken.None);

        _cacheMock.Verify(
            c =>
                c.RemoveAsync(
                    CatalogCacheKeys.Authors,
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
    public async Task GivenAuthorUpdatedEvent_WhenHandling_ThenShouldEvictAuthorsAndBooks()
    {
        await _handler.Handle(new AuthorUpdatedEvent(), CancellationToken.None);

        VerifyEvictedWithBooks();
    }

    [Test]
    public async Task GivenAuthorDeletedEvent_WhenHandling_ThenShouldEvictAuthorsAndBooks()
    {
        await _handler.Handle(new AuthorDeletedEvent(), CancellationToken.None);

        VerifyEvictedWithBooks();
    }

    private void VerifyEvictedWithBooks()
    {
        _cacheMock.Verify(
            c =>
                c.RemoveAsync(
                    CatalogCacheKeys.Authors,
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
