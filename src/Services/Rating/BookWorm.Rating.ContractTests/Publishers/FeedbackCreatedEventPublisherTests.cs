using BookWorm.Contracts;

namespace BookWorm.Rating.ContractTests.Publishers;

[Category("PactProvider")]
public sealed class FeedbackCreatedEventPublisherTests
{
    [Test]
    public void GivenFeedbackCreatedIntegrationEvent_WhenPublished_ThenShouldMatchContract()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 4;

        var @event = new FeedbackCreatedIntegrationEvent(bookId, rating, feedbackId);

        // Assert
        PactTests.Helper.VerifyProviderMessage("Catalog", "Rating", @event);
    }
}
