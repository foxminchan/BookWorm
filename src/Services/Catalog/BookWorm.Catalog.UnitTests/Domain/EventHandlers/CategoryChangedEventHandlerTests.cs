using BookWorm.Catalog.Domain.EventHandlers;
using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.UnitTests.Domain.EventHandlers;

public sealed class CategoryChangedEventHandlerTests
{
    private readonly Mock<IFusionCache> _cacheMock = new();
    private readonly CategoryChangedEventHandler _handler;

    public CategoryChangedEventHandlerTests()
    {
        _handler = new(_cacheMock.Object);
    }

    [Test]
    public async Task GivenCategoryCreatedEvent_WhenHandling_ThenShouldRemoveCategoriesKey()
    {
        await _handler.Handle(new CategoryCreatedEvent(), CancellationToken.None);

        _cacheMock.Verify(
            c =>
                c.RemoveAsync(
                    CatalogCacheKeys.Categories,
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
    public async Task GivenCategoryUpdatedEvent_WhenHandling_ThenShouldEvictCategoriesAndBooks()
    {
        await _handler.Handle(new CategoryUpdatedEvent(), CancellationToken.None);

        VerifyEvictedWithBooks();
    }

    [Test]
    public async Task GivenCategoryDeletedEvent_WhenHandling_ThenShouldEvictCategoriesAndBooks()
    {
        await _handler.Handle(new CategoryDeletedEvent(), CancellationToken.None);

        VerifyEvictedWithBooks();
    }

    private void VerifyEvictedWithBooks()
    {
        _cacheMock.Verify(
            c =>
                c.RemoveAsync(
                    CatalogCacheKeys.Categories,
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
