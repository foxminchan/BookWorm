using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class OutboxFilterSpecTests
{
    [Test]
    public void GivenOutboxFilterSpec_WhenCreated_ThenShouldFilterBySentStatus()
    {
        // Act
        var spec = new OutboxFilterSpec();

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.OrderExpressions.ShouldNotBeEmpty();
    }
}
