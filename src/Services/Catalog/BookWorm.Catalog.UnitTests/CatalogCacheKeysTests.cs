using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

namespace BookWorm.Catalog.UnitTests;

public sealed class CatalogCacheKeysTests
{
    [Test]
    public void GivenGuid_WhenBookDetailCalled_ThenShouldFormatKeyWithBookTag()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var key = CatalogCacheKeys.BookDetail(id);

        // Assert
        key.ShouldBe($"{CatalogCacheKeys.BookTag}:{id}");
    }

    [Test]
    public void GivenBookId_WhenBookDetailCalled_ThenShouldFormatKeyWithBookTag()
    {
        // Arrange
        var id = BookId.From(Guid.CreateVersion7());

        // Act
        var key = CatalogCacheKeys.BookDetail(id);

        // Assert
        key.ShouldBe($"{CatalogCacheKeys.BookTag}:{id}");
    }

    [Test]
    public void GivenGuidAndBookId_WhenBookSummaryCalled_ThenShouldFormatKeyWithBookTagAndSummaryPrefix()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var bookId = BookId.From(Guid.CreateVersion7());

        // Act
        var guidKey = CatalogCacheKeys.BookSummary(guid);
        var bookIdKey = CatalogCacheKeys.BookSummary(bookId);

        // Assert
        guidKey.ShouldBe($"{CatalogCacheKeys.BookTag}:summary:{guid}");
        bookIdKey.ShouldBe($"{CatalogCacheKeys.BookTag}:summary:{bookId}");
    }

    [Test]
    public void GivenSearchAndPageSize_WhenBookSearchCalled_ThenShouldFormatKeyWithHash()
    {
        // Arrange
        var search = "clean code";
        var pageSize = 10;

        // Act
        var key = CatalogCacheKeys.BookSearch(search, pageSize);

        // Assert
        key.ShouldStartWith("catalog:books:search:v1:");
        key.ShouldEndWith($":{pageSize}");
    }

    [Test]
    public void GivenCanonicalKey_WhenBookListCalled_ThenShouldFormatKeyWithHash()
    {
        // Arrange
        var canonicalKey = "pageIndex=1&pageSize=20";

        // Act
        var key = CatalogCacheKeys.BookList(canonicalKey);

        // Assert
        key.ShouldStartWith("catalog:books:list:v1:");
    }
}
