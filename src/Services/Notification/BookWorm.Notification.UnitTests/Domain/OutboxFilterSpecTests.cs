using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class OutboxFilterSpecTests
{
    [Test]
    public void GivenOutboxFilterSpec_WhenCreated_ThenShouldHaveWhereAndOrderExpressions()
    {
        // Act
        var spec = new OutboxFilterSpec();

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenSentOutbox_WhenEvaluatingFilter_ThenShouldMatch()
    {
        // Arrange
        var spec = new OutboxFilterSpec();
        var outbox = new Outbox("User", "user@example.com", "Subject", "Body");
        outbox.MarkAsSent();

        var filter = spec.WhereExpressions.First().Filter.Compile();

        // Act
        var result = filter(outbox);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenUnsentOutbox_WhenEvaluatingFilter_ThenShouldNotMatch()
    {
        // Arrange
        var spec = new OutboxFilterSpec();
        var outbox = new Outbox("User", "user@example.com", "Subject", "Body");

        var filter = spec.WhereExpressions.First().Filter.Compile();

        // Act
        var result = filter(outbox);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenOutboxFilterSpec_WhenEvaluatingOrder_ThenShouldOrderBySequenceNumber()
    {
        // Arrange
        var spec = new OutboxFilterSpec();
        var outbox = new Outbox("User", "user@example.com", "Subject", "Body")
        {
            SequenceNumber = 42,
        };

        var orderExpression = spec.OrderExpressions.First();
        var keySelector = orderExpression.KeySelector.Compile();

        // Act
        var key = keySelector(outbox);

        // Assert
        key.ShouldBe(42L as object);
    }
}
