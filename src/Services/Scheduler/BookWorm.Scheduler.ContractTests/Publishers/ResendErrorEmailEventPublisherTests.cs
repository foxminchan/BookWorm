using BookWorm.Common;
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
        PactTestHelper.VerifyProviderMessage("Notification", "Scheduler", @event);
    }
}
