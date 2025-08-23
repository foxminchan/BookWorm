using BookWorm.Rating.Domain.Events;
using BookWorm.Rating.Domain.Exceptions;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Rating.UnitTests.Domain;

public sealed class FeedbackAggregatorTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingFeedback_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
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
        var bookId = Guid.CreateVersion7();
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
        var bookId = Guid.CreateVersion7();
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
        var bookId = Guid.CreateVersion7();
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
        var bookId = Guid.CreateVersion7();
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
        var bookId = Guid.CreateVersion7();
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

    [Test]
    [Arguments(0)]
    [Arguments(5)]
    public void GivenFeedbackWithBoundaryRatings_WhenCreating_ThenShouldHandleCorrectly(
        int boundaryRating
    )
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const string firstName = "John";
        const string lastName = "Doe";
        const string comment = "Boundary rating test";

        // Act
        var feedback = new Feedback(bookId, firstName, lastName, comment, boundaryRating);

        // Assert
        feedback.BookId.ShouldBe(bookId);
        feedback.FirstName.ShouldBe(firstName);
        feedback.LastName.ShouldBe(lastName);
        feedback.Comment.ShouldBe(comment);
        feedback.Rating.ShouldBe(boundaryRating);

        // Verify domain event is still registered for boundary values
        var domainEvents = feedback.DomainEvents;
        domainEvents.Count.ShouldBe(1);

        var createdEvent = domainEvents.First() as FeedbackCreatedEvent;
        createdEvent.ShouldNotBeNull();
        createdEvent.BookId.ShouldBe(bookId);
        createdEvent.Rating.ShouldBe(boundaryRating);
        createdEvent.FeedbackId.ShouldBe(feedback.Id);
    }

    [Test]
    public void GivenFeedbackWithDomainEvents_WhenClearing_ThenEventsShouldBeEmpty()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int rating = 4;
        var feedback = new Feedback(bookId, "Jane", "Smith", "Good book", rating);

        // Verify we have domain events initially
        feedback.DomainEvents.Count.ShouldBeGreaterThan(0);

        // Act
        feedback.ClearDomainEvents();

        // Assert
        feedback.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void GivenExistingFeedback_WhenRemoving_ThenShouldReturnSameInstance()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int rating = 3;
        var feedback = new Feedback(bookId, "John", "Doe", "Test feedback", rating);

        // Act
        var result = feedback.Remove();

        // Assert
        result.ShouldBeSameAs(feedback);
    }
}
