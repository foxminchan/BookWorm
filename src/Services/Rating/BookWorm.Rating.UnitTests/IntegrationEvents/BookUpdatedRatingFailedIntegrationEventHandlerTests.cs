using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.IntegrationEvents.EventHandlers;

namespace BookWorm.Rating.UnitTests.IntegrationEvents;

public sealed class BookUpdatedRatingFailedIntegrationEventHandlerTests
{
    private readonly BookUpdatedRatingFailedIntegrationEventHandler _handler;
    private readonly Mock<IFeedbackRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public BookUpdatedRatingFailedIntegrationEventHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenFeedbackExists_WhenHandling_ThenShouldDeleteAndSave()
    {
        var feedbackId = Guid.CreateVersion7();
        var @event = new BookUpdatedRatingFailedIntegrationEvent(feedbackId);

        var feedback = new Feedback(Guid.CreateVersion7(), "John", "Doe", "Great book", 5);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.Handle(@event, CancellationToken.None);

        _repositoryMock.Verify(x => x.Delete(feedback), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenFeedbackNotFound_WhenHandling_ThenShouldReturnWithoutDeleting()
    {
        var feedbackId = Guid.CreateVersion7();
        var @event = new BookUpdatedRatingFailedIntegrationEvent(feedbackId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        await _handler.Handle(@event, CancellationToken.None);

        _repositoryMock.Verify(x => x.Delete(It.IsAny<Feedback>()), Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
