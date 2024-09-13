namespace BookWorm.Rating.UnitTests.Domain;

public sealed class FeedbackTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void GivenValidParameters_ShouldPropertiesShouldBeSetCorrectly_WhenCreatingFeedback(int rating)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const string comment = "Great book!";
        var userId = Guid.NewGuid();

        // Act
        var feedback = new Feedback(bookId, rating, comment, userId);

        // Assert
        feedback.BookId.Should().Be(bookId);
        feedback.Rating.Should().Be(rating);
        feedback.Comment.Should().Be(comment);
        feedback.UserId.Should().Be(userId);
        feedback.IsHidden.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void GivenEdgeCaseRating_ShouldRatingShouldBeValid_WhenCreatingFeedback(int rating)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var feedback = new Feedback(bookId, rating, null, userId);

        // Assert
        feedback.Rating.Should().Be(rating);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void GivenInvalidRating_ShouldThrowArgumentOutOfRangeException_WhenCreatingFeedback(int rating)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        Func<Feedback> act = () => new(bookId, rating, null, userId);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*rating*");
    }

    [Fact]
    public void GivenValidParameters_ShouldSetIsHiddenBeTrue_WhenHidingFeedback()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var feedback = new Feedback(bookId, 4, "Interesting book", Guid.NewGuid());

        // Act
        feedback.Hide();

        // Assert
        feedback.IsHidden.Should().BeTrue();
    }

    [Theory]
    [CombinatorialData]
    public void GivenInvalidParameters_ShouldThrowArgumentException_WhenCreatingFeedback(
        [CombinatorialValues("00000000-0000-0000-0000-000000000000")]
        Guid invalidGuid,
        [CombinatorialValues(-1, 6)] int invalidRating)
    {
        // Act
        Func<Feedback> actBookId = () => new(invalidGuid, 3, null, Guid.NewGuid());
        Func<Feedback> actRating = () => new(Guid.NewGuid(), invalidRating, null, Guid.NewGuid());
        Func<Feedback> actUserId = () => new(Guid.NewGuid(), 3, null, invalidGuid);

        // Assert
        actBookId.Should().Throw<ArgumentException>()
            .WithMessage("*bookId*");
        actRating.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*rating*");
        actUserId.Should().Throw<ArgumentException>()
            .WithMessage("*userId*");
    }
}
