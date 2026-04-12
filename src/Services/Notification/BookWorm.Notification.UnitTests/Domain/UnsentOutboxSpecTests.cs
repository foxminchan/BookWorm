using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class UnsentOutboxSpecTests
{
    [Test]
    public void GivenUnsentOutboxSpec_WhenCreated_ThenShouldHaveWhereAndOrderExpressions()
    {
        // Act
        var spec = new UnsentOutboxSpec();

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.OrderExpressions.ShouldNotBeEmpty();
        spec.AsTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenUnsentOutbox_WhenEvaluatingFilter_ThenShouldMatch()
    {
        // Arrange
        var spec = new UnsentOutboxSpec();
        var outbox = new Outbox("User", "user@example.com", "Subject", "Body");

        var filter = spec.WhereExpressions.First().Filter.Compile();

        // Act
        var result = filter(outbox);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenSentOutbox_WhenEvaluatingFilter_ThenShouldNotMatch()
    {
        // Arrange
        var spec = new UnsentOutboxSpec();
        var outbox = new Outbox("User", "user@example.com", "Subject", "Body");
        outbox.MarkAsSent();

        var filter = spec.WhereExpressions.First().Filter.Compile();

        // Act
        var result = filter(outbox);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenUnsentOutboxSpec_WhenEvaluatingOrder_ThenShouldOrderBySequenceNumber()
    {
        // Arrange
        var spec = new UnsentOutboxSpec();
        var outbox = new Outbox("User", "user@example.com", "Subject", "Body")
        {
            SequenceNumber = 99,
        };

        var orderExpression = spec.OrderExpressions.First();
        var keySelector = orderExpression.KeySelector.Compile();

        // Act
        var key = keySelector(outbox);

        // Assert
        key.ShouldBe(99L);
    }
}
