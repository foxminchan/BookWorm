using BookWorm.Notification.Domain.Models;
using BookWorm.SharedKernel.Helpers;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class OutboxTests
{
    [Test]
    public void GivenOutbox_WhenCreated_ThenPropertiesShouldBeSet()
    {
        // Arrange
        const string toName = "Test User";
        const string toEmail = "test@example.com";
        const string subject = "Test Subject";
        const string body = "Test Body";

        // Act
        var outbox = new Outbox(toName, toEmail, subject, body);

        // Assert
        outbox.Id.ShouldBe(Guid.Empty);
        outbox.ToName.ShouldBe(toName);
        outbox.ToEmail.ShouldBe(toEmail);
        outbox.Subject.ShouldBe(subject);
        outbox.Body.ShouldBe(body);
        outbox.IsSent.ShouldBeFalse();
        outbox.SequenceNumber.ShouldBe(0);
        outbox.SentAt.ShouldBeNull();
        outbox.CreatedAt.ShouldBeLessThanOrEqualTo(DateTimeHelper.UtcNow());
    }

    [Test]
    public void GivenOutbox_WhenMarkedAsSent_ThenPropertiesShouldBeUpdated()
    {
        // Arrange
        var outbox = new Outbox("Test User", "test@example.com", "Test Subject", "Test Body");

        // Act
        outbox.MarkAsSent();

        // Assert
        outbox.IsSent.ShouldBeTrue();
        outbox.SentAt.ShouldNotBeNull();
        outbox.SentAt?.ShouldBeLessThanOrEqualTo(DateTimeHelper.UtcNow());
    }

    [Test]
    public void GivenOutbox_WhenMarkingAsSent_ThenShouldReturnSameInstance()
    {
        // Arrange
        var outbox = new Outbox("Test User", "test@example.com", "Test Subject", "Test Body");

        // Act
        var result = outbox.MarkAsSent();

        // Assert
        result.ShouldBeSameAs(outbox);
    }

    [Test]
    public void GivenOutbox_WhenCreated_ThenIdShouldBeEmpty()
    {
        // Act
        var outbox = new Outbox("Test User", "test@example.com", "Test Subject", "Test Body");

        // Assert
        outbox.Id.ShouldBe(Guid.Empty); // Id is database-generated via HasDefaultValueSql
    }

    [Test]
    public void GivenOutbox_WhenCreated_ThenSequenceNumberShouldBeInitializable()
    {
        // Arrange
        const long expectedSequenceNumber = 12345;

        // Act
        var outbox = new Outbox("Test User", "test@example.com", "Test Subject", "Test Body")
        {
            SequenceNumber = expectedSequenceNumber,
        };

        // Assert
        outbox.SequenceNumber.ShouldBe(expectedSequenceNumber);
    }
}
