using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class BookFilterSpecTests
{
    [Test]
    public void GivenNullFilters_WhenCreatingCategoryPublisherSpec_ThenShouldSetAsNoTracking()
    {
        // Act
        var spec = new BookFilterSpec(null, null);

        // Assert
        spec.AsNoTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenCategoryIds_WhenCreatingSpec_ThenShouldApplyFilter()
    {
        // Arrange
        var categoryIds = new[] { Guid.CreateVersion7() };

        // Act
        var spec = new BookFilterSpec(categoryIds, null);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenPublisherIds_WhenCreatingSpec_ThenShouldApplyFilter()
    {
        // Arrange
        var publisherIds = new[] { Guid.CreateVersion7() };

        // Act
        var spec = new BookFilterSpec(null, publisherIds);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenBothFilters_WhenCreatingSpec_ThenShouldApplyBothFilters()
    {
        // Arrange
        var categoryIds = new[] { Guid.CreateVersion7() };
        var publisherIds = new[] { Guid.CreateVersion7() };

        // Act
        var spec = new BookFilterSpec(categoryIds, publisherIds);

        // Assert
        spec.WhereExpressions.Count().ShouldBe(2);
    }

    [Test]
    public void GivenEmptyCategoryIds_WhenCreatingSpec_ThenShouldNotApplyFilter()
    {
        // Act
        var spec = new BookFilterSpec([], null);

        // Assert
        spec.WhereExpressions.ShouldBeEmpty();
    }

    [Test]
    public void GivenMinPrice_WhenCreatingExtendedSpec_ThenShouldApplyFilter()
    {
        // Act
        var spec = new BookFilterSpec(10.00m, null, null, null, null, null);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenMaxPrice_WhenCreatingExtendedSpec_ThenShouldApplyFilter()
    {
        // Act
        var spec = new BookFilterSpec(null, 50.00m, null, null, null, null);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenMinAndMaxPrice_WhenCreatingExtendedSpec_ThenShouldApplyBothFilters()
    {
        // Act
        var spec = new BookFilterSpec(10.00m, 50.00m, null, null, null, null);

        // Assert
        spec.WhereExpressions.Count().ShouldBe(2);
    }

    [Test]
    public void GivenIds_WhenCreatingExtendedSpec_ThenShouldApplyFilter()
    {
        // Arrange
        var ids = new[] { Guid.CreateVersion7() };

        // Act
        var spec = new BookFilterSpec(null, null, null, null, null, ids);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenAuthorIds_WhenCreatingExtendedSpec_ThenShouldApplyFilter()
    {
        // Arrange
        var authorIds = new[] { Guid.CreateVersion7() };

        // Act
        var spec = new BookFilterSpec(null, null, null, null, authorIds, null);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenIdsOnlyConstructor_WhenCreatingSpec_ThenShouldApplyIdFilter()
    {
        // Arrange
        var ids = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };

        // Act
        var spec = new BookFilterSpec(ids);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenNullIdsOnlyConstructor_WhenCreatingSpec_ThenShouldNotApplyFilter()
    {
        // Act
        var spec = new BookFilterSpec(ids: null);

        // Assert
        spec.WhereExpressions.ShouldBeEmpty();
    }

    [Test]
    public void GivenPagination_WhenCreatingPagedSpec_ThenShouldApplyPaging()
    {
        // Arrange
        const int pageIndex = 2;
        const int pageSize = 10;

        // Act
        var spec = new BookFilterSpec(
            pageIndex,
            pageSize,
            null,
            false,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.Skip.ShouldBe((pageIndex - 1) * pageSize);
        spec.Take.ShouldBe(pageSize);
    }

    [Test]
    public void GivenAllFilters_WhenCreatingPagedSpec_ThenShouldApplyAllFilters()
    {
        // Arrange
        var categoryIds = new[] { Guid.CreateVersion7() };
        var publisherIds = new[] { Guid.CreateVersion7() };
        var authorIds = new[] { Guid.CreateVersion7() };
        var ids = new[] { Guid.CreateVersion7() };

        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Book.Name),
            true,
            10.00m,
            100.00m,
            categoryIds,
            publisherIds,
            authorIds,
            ids
        );

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.OrderExpressions.ShouldNotBeEmpty();
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(10);
        spec.IncludeExpressions.ShouldNotBeEmpty();
    }
}
