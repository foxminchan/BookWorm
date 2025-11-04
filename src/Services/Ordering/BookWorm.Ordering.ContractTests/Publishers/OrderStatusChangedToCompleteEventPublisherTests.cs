using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Ordering.ContractTests.Publishers;

public sealed class OrderStatusChangedToCompleteEventPublisherTests : SnapshotTestBase
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

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(@event);

            // Assert
            await VerifySnapshot(harness);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
