using BookWorm.Chassis.EventBus.Serialization;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Ordering.ContractTests.Publishers;

public sealed class OrderStatusChangedToCompleteEventPublisherTests
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x =>
                x.UsingInMemory(
                    (context, cfg) =>
                    {
                        cfg.UseCloudEvents();
                        cfg.ConfigureEndpoints(context);
                    }
                )
            )
            .BuildServiceProvider(true);

        _harness = await _provider.StartTestHarness();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

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

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await SnapshotTestHelper.Verify(_harness);
    }
}
