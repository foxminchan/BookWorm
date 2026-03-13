using BookWorm.Chassis.EventBus.Serialization;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Rating.ContractTests.Publishers;

public sealed class FeedbackDeletedEventPublisherTests
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
    public async Task GivenFeedbackDeletedIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 5;

        var @event = new FeedbackDeletedIntegrationEvent(bookId, rating, feedbackId);

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await SnapshotTestHelper.Verify(_harness);
    }
}
