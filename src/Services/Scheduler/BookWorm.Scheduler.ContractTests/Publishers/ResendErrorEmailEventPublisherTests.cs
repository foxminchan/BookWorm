using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Scheduler.ContractTests.Publishers;

public sealed class ResendErrorEmailEventPublisherTests : SnapshotTestBase
{
    [Test]
    public async Task GivenResendErrorEmailIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(@event);

        // Assert
        (await harness.Published.Any<ResendErrorEmailIntegrationEvent>()).ShouldBeTrue();

        var publishedMessage = harness.Published.Select<ResendErrorEmailIntegrationEvent>().First();

        // Verify contract structure - event has no custom properties, only base IntegrationEvent properties
        await VerifySnapshot(publishedMessage.Context.Message);

        await harness.Stop();
    }
}
