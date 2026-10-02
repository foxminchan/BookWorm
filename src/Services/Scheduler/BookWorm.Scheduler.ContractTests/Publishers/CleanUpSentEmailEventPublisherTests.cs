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
        PactTests.Helper.VerifyProviderMessage("Notification", "Scheduler", @event);
    }
}
