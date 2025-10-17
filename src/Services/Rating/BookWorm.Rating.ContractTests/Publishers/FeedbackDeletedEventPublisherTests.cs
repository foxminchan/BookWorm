using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Rating.ContractTests.Publishers;

public sealed class FeedbackDeletedEventPublisherTests : SnapshotTestBase
{
    [Test]
    public async Task GivenFeedbackDeletedIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 5;

        var @event = new FeedbackDeletedIntegrationEvent(bookId, rating, feedbackId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(@event);

        // Assert
        (await harness.Published.Any<FeedbackDeletedIntegrationEvent>()).ShouldBeTrue();

        var publishedMessage = harness.Published.Select<FeedbackDeletedIntegrationEvent>().First();

        // Verify contract structure
        await VerifySnapshot(publishedMessage.Context.Message);

        await harness.Stop();
    }
}
