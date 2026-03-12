using BookWorm.Common;
using BookWorm.Contracts;

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

        // Assert
        await SnapshotTestHelper.Verify(@event);
    }
}
