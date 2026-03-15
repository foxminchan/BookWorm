using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Features;
using BookWorm.Rating.UnitTests.Fakers;

namespace BookWorm.Rating.UnitTests.Domain;

public sealed class DomainToDtoMapperTests
{
    [Test]
    public void GivenSingleFeedback_WhenMappingToDtos_ThenShouldMapAllProperties()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const string firstName = "John";
        const string lastName = "Doe";
        const string comment = "Great book!";
        const int rating = 5;
        var feedback = new Feedback(bookId, firstName, lastName, comment, rating);

        // Act
        var result = new[] { feedback }.ToFeedbackDtos();

        // Assert
        result.Count.ShouldBe(1);
        var dto = result[0];
        dto.Id.ShouldBe(feedback.Id);
        dto.FirstName.ShouldBe(firstName);
        dto.LastName.ShouldBe(lastName);
        dto.Comment.ShouldBe(comment);
        dto.Rating.ShouldBe(rating);
        dto.BookId.ShouldBe(bookId);
    }

    [Test]
    public void GivenMultipleFeedbacks_WhenMappingToDtos_ThenShouldMapAllItems()
    {
        // Arrange
        var feedbacks = new FeedbackFaker().Generate();

        // Act
        var result = feedbacks.ToFeedbackDtos();

        // Assert
        result.Count.ShouldBe(feedbacks.Length);
        for (var i = 0; i < feedbacks.Length; i++)
        {
            result[i].Id.ShouldBe(feedbacks[i].Id);
            result[i].BookId.ShouldBe(feedbacks[i].BookId);
            result[i].Rating.ShouldBe(feedbacks[i].Rating);
            result[i].FirstName.ShouldBe(feedbacks[i].FirstName);
            result[i].LastName.ShouldBe(feedbacks[i].LastName);
            result[i].Comment.ShouldBe(feedbacks[i].Comment);
        }
    }

    [Test]
    public void GivenEmptyCollection_WhenMappingToDtos_ThenShouldReturnEmptyList()
    {
        // Arrange
        var feedbacks = Enumerable.Empty<Feedback>();

        // Act
        var result = feedbacks.ToFeedbackDtos();

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public void GivenFeedbackWithNullValues_WhenMappingToDtos_ThenShouldPreserveNulls()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedback = new Feedback(bookId, null, null, null, 3);

        // Act
        var result = new[] { feedback }.ToFeedbackDtos();

        // Assert
        result.Count.ShouldBe(1);
        var dto = result[0];
        dto.FirstName.ShouldBeNull();
        dto.LastName.ShouldBeNull();
        dto.Comment.ShouldBeNull();
        dto.Rating.ShouldBe(3);
        dto.BookId.ShouldBe(bookId);
    }
}
