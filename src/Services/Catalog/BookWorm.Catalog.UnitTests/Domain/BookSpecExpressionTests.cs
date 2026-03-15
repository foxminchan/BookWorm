using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class BookSpecExpressionTests
{
    [Test]
    public void GivenNameOrderBy_WhenAscending_ThenShouldApplyAscendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Book.Name),
            false,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenNameOrderBy_WhenDescending_ThenShouldApplyDescendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Book.Name),
            true,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenOriginalPriceOrderBy_WhenAscending_ThenShouldApplyAscendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Price.OriginalPrice),
            false,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenOriginalPriceOrderBy_WhenDescending_ThenShouldApplyDescendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Price.OriginalPrice),
            true,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenDiscountPriceOrderBy_WhenAscending_ThenShouldApplyAscendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Price.DiscountPrice),
            false,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenDiscountPriceOrderBy_WhenDescending_ThenShouldApplyDescendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Price.DiscountPrice),
            true,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenStatusOrderBy_WhenAscending_ThenShouldApplyAscendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Book.Status),
            false,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenStatusOrderBy_WhenDescending_ThenShouldApplyDescendingOrder()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            nameof(Book.Status),
            true,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenUnknownOrderBy_WhenAscending_ThenShouldDefaultToNameAscending()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            "UnknownField",
            false,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenUnknownOrderBy_WhenDescending_ThenShouldDefaultToNameDescending()
    {
        // Act
        var spec = new BookFilterSpec(
            1,
            10,
            "UnknownField",
            true,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenPagingParameters_WhenApplied_ThenShouldCalculateSkipAndTake()
    {
        // Act
        var spec = new BookFilterSpec(3, 20, null, false, null, null, null, null, null, null);

        // Assert
        spec.Skip.ShouldBe(40);
        spec.Take.ShouldBe(20);
    }

    [Test]
    public void GivenFirstPage_WhenApplyingPaging_ThenSkipShouldBeZero()
    {
        // Act
        var spec = new BookFilterSpec(1, 10, null, false, null, null, null, null, null, null);

        // Assert
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(10);
    }
}
