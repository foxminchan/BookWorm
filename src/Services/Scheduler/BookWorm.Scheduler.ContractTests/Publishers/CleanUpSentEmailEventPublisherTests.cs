using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Scheduler.ContractTests.Publishers;

public sealed class CleanUpSentEmailEventPublisherTests : SnapshotTestBase
{
    [Test]
    public async Task GivenCleanUpSentEmailIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var @event = new CleanUpSentEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(@event);

        // Assert
        (await harness.Published.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

        var publishedMessage = harness.Published.Select<CleanUpSentEmailIntegrationEvent>().First();

        // Verify contract structure - event has no custom properties, only base IntegrationEvent properties
        await VerifySnapshot(publishedMessage.Context.Message);

        await harness.Stop();
    }
}
