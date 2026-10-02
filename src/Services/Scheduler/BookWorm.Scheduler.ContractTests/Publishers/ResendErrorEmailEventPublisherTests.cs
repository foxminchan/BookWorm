using BookWorm.Contracts;

namespace BookWorm.Scheduler.ContractTests.Publishers;

[Category("PactProvider")]
public sealed class ResendErrorEmailEventPublisherTests
{
    [Test]
    public void GivenResendErrorEmailIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new ResendErrorEmailIntegrationEvent();

        // Assert
        PactTests.Helper.VerifyProviderMessage("Notification", "Scheduler", @event);
    }
}
