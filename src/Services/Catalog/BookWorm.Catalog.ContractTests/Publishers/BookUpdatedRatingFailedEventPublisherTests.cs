using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.Repository;
using BookWorm.Contracts;

namespace BookWorm.Catalog.ContractTests.Publishers;

[Category("PactProvider")]
public sealed class BookUpdatedRatingFailedEventPublisherTests
{
    private const int Rating = 4;
    private Guid _bookId;
    private Guid _feedbackId;
    private Mock<IBookRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public void SetUp()
    {
        _bookId = Guid.CreateVersion7();
        _feedbackId = Guid.CreateVersion7();

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GivenNonExistingBook_WhenHandlingFeedbackCreated_ThenShouldPublishFailedEvent()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var @event = new FeedbackCreatedIntegrationEvent(_bookId, Rating, _feedbackId);
        var handler = new FeedbackCreatedIntegrationEventHandler(_repositoryMock.Object);

        // Act
        var publishedEvent = await handler.Handle(@event, CancellationToken.None)
            ?? throw new InvalidOperationException("Expected rating failure integration event.");

        // Assert
        PactTests.Helper.VerifyProviderMessage("Rating", "Catalog", publishedEvent);
        _repositoryMock.Verify(
            x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
