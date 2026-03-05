using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Scheduler.ContractTests.Publishers;

public sealed class ResendErrorEmailEventPublisherTests
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness()
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
    public async Task GivenResendErrorEmailIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new ResendErrorEmailIntegrationEvent();

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await SnapshotTestHelper.Verify(_harness);
    }
}
