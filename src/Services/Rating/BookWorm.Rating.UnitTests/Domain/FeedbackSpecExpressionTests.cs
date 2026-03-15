using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;

namespace BookWorm.Rating.UnitTests.Domain;

public sealed class FeedbackSpecExpressionTests
{
    [Test]
    public void GivenRatingOrderBy_WhenAscending_ThenShouldApplyAscendingOrder()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, nameof(Feedback.Rating), false);

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenRatingOrderBy_WhenDescending_ThenShouldApplyDescendingOrder()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, nameof(Feedback.Rating), true);

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenCreatedAtOrderBy_WhenAscending_ThenShouldApplyAscendingOrder()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, nameof(Feedback.CreatedAt), false);

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenCreatedAtOrderBy_WhenDescending_ThenShouldApplyDescendingOrder()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, nameof(Feedback.CreatedAt), true);

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenUnknownOrderBy_WhenAscending_ThenShouldDefaultToRatingAscending()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, "UnknownField", false);

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenUnknownOrderBy_WhenDescending_ThenShouldDefaultToRatingDescending()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, "UnknownField", true);

        // Assert
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenPagingParameters_WhenApplied_ThenShouldCalculateSkipAndTake()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int pageIndex = 3;
        const int pageSize = 10;

        // Act
        var spec = new FeedbackFilterSpec(bookId, null, false, pageIndex, pageSize);

        // Assert
        spec.Skip.ShouldBe(20);
        spec.Take.ShouldBe(10);
    }

    [Test]
    public void GivenEmptyStringOrderBy_WhenCreatingFilterSpec_ThenShouldNotApplyOrdering()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, "", false);

        // Assert
        spec.OrderExpressions.ShouldBeEmpty();
    }

    [Test]
    public void GivenWhitespaceOrderBy_WhenCreatingFilterSpec_ThenShouldNotApplyOrdering()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        // Act
        var spec = new FeedbackFilterSpec(bookId, "   ", false);

        // Assert
        spec.OrderExpressions.ShouldBeEmpty();
    }
}
