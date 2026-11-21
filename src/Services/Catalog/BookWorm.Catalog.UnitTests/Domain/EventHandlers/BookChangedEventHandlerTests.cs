using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
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
    public async Task GivenBookChangedEvent_WhenHandling_ThenShouldRemoveAllBookCacheEntriesAndTags()
    {
        // Arrange
        var bookId = BookId.From(Guid.CreateVersion7());
        var @event = new BookChangedEvent(bookId);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    CatalogCacheKeys.BookDetail(bookId),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    CatalogCacheKeys.BookSummary(bookId),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            x =>
                x.RemoveByTagAsync(
                    CatalogCacheKeys.BookListTag,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            x =>
                x.RemoveByTagAsync(
                    CatalogCacheKeys.BookSearchTag,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDifferentBookIds_WhenHandling_ThenShouldRemoveCorrectKeys()
    {
        // Arrange
        var firstBookId = BookId.From(Guid.CreateVersion7());
        var secondBookId = BookId.From(Guid.CreateVersion7());

        // Act
        await _handler.Handle(new BookChangedEvent(firstBookId), CancellationToken.None);
        await _handler.Handle(new BookChangedEvent(secondBookId), CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    CatalogCacheKeys.BookDetail(firstBookId),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    CatalogCacheKeys.BookDetail(secondBookId),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    CatalogCacheKeys.BookSummary(firstBookId),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            x =>
                x.RemoveAsync(
                    CatalogCacheKeys.BookSummary(secondBookId),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenBookCreatedEvent_WhenHandling_ThenShouldRemoveListAndSearchTags()
    {
        // Arrange
        var @event = new BookCreatedEvent();

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            x =>
                x.RemoveByTagAsync(
                    CatalogCacheKeys.BookListTag,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _cacheMock.Verify(
            x =>
                x.RemoveByTagAsync(
                    CatalogCacheKeys.BookSearchTag,
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
