using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class UnsentOutboxSpecTests
{
    [Test]
    public void GivenUnsentOutboxSpec_WhenCreated_ThenShouldFilterByUnsentStatus()
    {
        // Act
        var spec = new UnsentOutboxSpec();

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.OrderExpressions.ShouldNotBeEmpty();
    }
}
