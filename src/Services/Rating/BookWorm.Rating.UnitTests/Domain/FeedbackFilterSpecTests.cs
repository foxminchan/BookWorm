using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;

namespace BookWorm.Rating.UnitTests.Domain;

public sealed class FeedbackFilterSpecTests
{
    [Test]
    public void GivenBookId_WhenCreatingFilterSpec_ThenShouldSetAsNoTracking()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, null, false);

        // Assert
        spec.AsNoTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenBookId_WhenCreatingFilterSpec_ThenShouldFilterByBookId()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, null, false);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenNullOrderBy_WhenCreatingFilterSpec_ThenShouldNotApplyOrdering()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, null, false);

        // Assert
        spec.OrderExpressions.ShouldBeEmpty();
    }

    [Test]
    public void GivenRatingOrderBy_WhenCreatingFilterSpec_ThenShouldApplyOrdering()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, nameof(Feedback.Rating), false);

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenPagination_WhenCreatingFilterSpec_ThenShouldApplyPaging()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int pageIndex = 2;
        const int pageSize = 10;

        // Act
        var spec = new FeedbackFilterSpec(bookId, null, false, pageIndex, pageSize);

        // Assert
        spec.Skip.ShouldBe((pageIndex - 1) * pageSize);
        spec.Take.ShouldBe(pageSize);
    }

    [Test]
    public void GivenFirstPage_WhenCreatingFilterSpec_ThenSkipShouldBeZero()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int pageIndex = 1;
        const int pageSize = 10;

        // Act
        var spec = new FeedbackFilterSpec(bookId, null, false, pageIndex, pageSize);

        // Assert
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(pageSize);
    }

    [Test]
    public void GivenOrderByAndPagination_WhenCreatingFilterSpec_ThenShouldApplyBoth()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int pageIndex = 3;
        const int pageSize = 5;

        // Act
        var spec = new FeedbackFilterSpec(
            bookId,
            nameof(Feedback.Rating),
            true,
            pageIndex,
            pageSize
        );

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
        spec.Skip.ShouldBe((pageIndex - 1) * pageSize);
        spec.Take.ShouldBe(pageSize);
        spec.AsNoTracking.ShouldBeTrue();
    }
}
