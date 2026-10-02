using BookWorm.Common;
using BookWorm.Contracts;

namespace BookWorm.Scheduler.ContractTests.Publishers;

[Category("PactProvider")]
public sealed class CleanUpSentEmailEventPublisherTests
{
    [Test]
    public void GivenCleanUpSentEmailIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new CleanUpSentEmailIntegrationEvent();

        // Assert
        PactTestHelper.VerifyProviderMessage("Notification", "Scheduler", @event);
    }
}
