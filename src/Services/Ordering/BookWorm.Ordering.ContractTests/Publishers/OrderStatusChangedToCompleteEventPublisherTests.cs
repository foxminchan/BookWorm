using BookWorm.Common;
using BookWorm.Contracts;

namespace BookWorm.Ordering.ContractTests.Publishers;

public sealed class OrderStatusChangedToCompleteEventPublisherTests
{
    [Test]
    public async Task GivenOrderStatusChangedToCompleteIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const string fullName = "John Doe";
        const string email = "john.doe@example.com";
        const decimal totalMoney = 149.99m;

        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        // Assert
        await SnapshotTestHelper.Verify(@event);
    }
}
