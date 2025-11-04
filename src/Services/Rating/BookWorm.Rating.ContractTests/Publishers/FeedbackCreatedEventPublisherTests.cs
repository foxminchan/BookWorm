using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Rating.ContractTests.Publishers;

public sealed class FeedbackCreatedEventPublisherTests
{
    [Test]
    public async Task GivenFeedbackCreatedIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 4;

        var @event = new FeedbackCreatedIntegrationEvent(bookId, rating, feedbackId);

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
