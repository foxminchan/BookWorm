using BookWorm.Contracts;

namespace BookWorm.Scheduler.ContractTests.Publishers;

[Category("PactProvider")]
public sealed class ProcessVectorBatchEventPublisherTests
{
    [Test]
    public void GivenProcessVectorBatchIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new ProcessVectorBatchIntegrationEvent();

        // Assert
        PactTests.Helper.VerifyProviderMessage("Catalog", "Scheduler", @event);
    }
}
