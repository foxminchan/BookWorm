using BookWorm.Common;
using BookWorm.Contracts;

namespace BookWorm.Rating.ContractTests.Publishers;

[Category("PactProvider")]
public sealed class FeedbackDeletedEventPublisherTests
{
    [Test]
    public void GivenFeedbackDeletedIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 5;

        var @event = new FeedbackDeletedIntegrationEvent(bookId, rating, feedbackId);

        // Assert
        PactTestHelper.VerifyProviderMessage("Catalog", "Rating", @event);
    }
}
