using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Scheduler.ContractTests.Publishers;

public sealed class CleanUpSentEmailEventPublisherTests
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

        try
        {
            // Act
            await harness.Bus.Publish(@event);

            // Assert
            await SnapshotTestHelper.Verify(harness);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
