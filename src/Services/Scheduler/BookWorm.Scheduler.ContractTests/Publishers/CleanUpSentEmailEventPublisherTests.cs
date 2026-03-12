using BookWorm.Common;
using BookWorm.Contracts;

namespace BookWorm.Scheduler.ContractTests.Publishers;

public sealed class CleanUpSentEmailEventPublisherTests
{
    [Test]
    public async Task GivenCleanUpSentEmailIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new CleanUpSentEmailIntegrationEvent();

        // Assert
        await SnapshotTestHelper.Verify(@event);
    }
}
