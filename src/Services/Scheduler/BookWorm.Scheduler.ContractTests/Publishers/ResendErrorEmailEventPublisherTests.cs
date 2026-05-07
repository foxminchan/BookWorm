using BookWorm.Common;
using BookWorm.Contracts;

namespace BookWorm.Scheduler.ContractTests.Publishers;

public sealed class ResendErrorEmailEventPublisherTests
{
    [Test]
    public async Task GivenResendErrorEmailIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new ResendErrorEmailIntegrationEvent();

        // Assert
        await SnapshotTestHelper.Verify(@event);
    }
}
