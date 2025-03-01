using BookWorm.Rating.Domain.Events;
using BookWorm.Rating.Domain.Exceptions;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.SharedKernel.SeedWork.Model;

namespace BookWorm.Rating.UnitTests.Domain;

public sealed class FeedbackAggregatorTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingFeedback_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const string firstName = "John";
        const string lastName = "Doe";
        const string comment = "Great book!";
        const int rating = 5;

        // Act
        var feedback = new Feedback(bookId, firstName, lastName, comment, rating);

        // Assert
        feedback.BookId.ShouldBe(bookId);
        feedback.FirstName.ShouldBe(firstName);
        feedback.LastName.ShouldBe(lastName);
        feedback.Comment.ShouldBe(comment);
        feedback.Rating.ShouldBe(rating);
    }

    [Test]
    [Arguments(-1)]
    [Arguments(6)]
    public void GivenInvalidRating_WhenCreatingFeedback_ThenShouldThrowDomainException(
        int invalidRating
    )
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const string firstName = "John";
        const string lastName = "Doe";
        const string comment = "Great book!";

        // Act
        var act = () => new Feedback(bookId, firstName, lastName, comment, invalidRating);

        // Assert
        var exception = act.ShouldThrow<RatingDomainException>();
        exception.Message.ShouldBe("Rating must be between 0 and 5.");
    }

    [Test]
    public void GivenNewFeedback_WhenCreating_ThenShouldRegisterFeedbackCreatedEvent()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const int rating = 4;

        // Act
        var feedback = new Feedback(bookId, "John", "Doe", "Good read", rating);

        // Assert
        var domainEvents = feedback.DomainEvents;
        domainEvents.Count.ShouldBe(1);

        var createdEvent = domainEvents.First() as FeedbackCreatedEvent;
        createdEvent.ShouldNotBeNull();
        createdEvent.BookId.ShouldBe(bookId);
        createdEvent.Rating.ShouldBe(rating);
        createdEvent.FeedbackId.ShouldBe(feedback.Id);
    }

    [Test]
    public void GivenExistingFeedback_WhenRemoved_ThenShouldRegisterFeedbackDeletedEvent()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const int rating = 3;
        var feedback = new Feedback(bookId, "John", "Doe", "Decent book", rating);

        // Clear the creation event for cleaner testing
        feedback.ClearDomainEvents();

        // Act
        feedback.Remove();

        // Assert
        var domainEvents = feedback.DomainEvents;
        domainEvents.Count.ShouldBe(1);

        var deletedEvent = domainEvents.First() as FeedbackDeletedEvent;
        deletedEvent.ShouldNotBeNull();
        deletedEvent.BookId.ShouldBe(bookId);
        deletedEvent.Rating.ShouldBe(rating);
        deletedEvent.FeedbackId.ShouldBe(feedback.Id);
    }

    [Test]
    public void GivenFeedbackWithNullValues_WhenCreating_ThenShouldAcceptNullValues()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const int rating = 2;

        // Act
        var feedback = new Feedback(bookId, null, null, null, rating);

        // Assert
        feedback.BookId.ShouldBe(bookId);
        feedback.FirstName.ShouldBeNull();
        feedback.LastName.ShouldBeNull();
        feedback.Comment.ShouldBeNull();
        feedback.Rating.ShouldBe(rating);
    }

    [Test]
    public void GivenExistingFeedback_WhenAccessingProperties_ThenShouldRespectImmutability()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const string firstName = "John";
        const string lastName = "Doe";
        const string comment = "Great book!";
        const int rating = 5;

        // Act
        var feedback = new Feedback(bookId, firstName, lastName, comment, rating);

        // Assert
        // Verify properties are as expected
        feedback.BookId.ShouldBe(bookId);
        feedback.Rating.ShouldBe(rating);

        // Verify these are private setters (would fail to compile if we tried to set them)
        // This is more of a compilation check than a runtime test
        feedback.ShouldBeAssignableTo<IAggregateRoot>();
    }
}
