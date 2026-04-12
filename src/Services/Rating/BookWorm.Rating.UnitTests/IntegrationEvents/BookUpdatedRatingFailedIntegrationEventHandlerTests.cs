using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.IntegrationEvents.EventHandlers;
using MassTransit;

namespace BookWorm.Rating.UnitTests.IntegrationEvents;

public sealed class BookUpdatedRatingFailedIntegrationEventHandlerTests
{
    private readonly Mock<ConsumeContext<BookUpdatedRatingFailedIntegrationEvent>> _contextMock =
        new();

    private readonly BookUpdatedRatingFailedIntegrationEventHandler _handler;
    private readonly Mock<IFeedbackRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public BookUpdatedRatingFailedIntegrationEventHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenFeedbackExists_WhenConsuming_ThenShouldDeleteAndSave()
    {
        // Arrange
        var feedbackId = Guid.CreateVersion7();
        var @event = new BookUpdatedRatingFailedIntegrationEvent(feedbackId);
        _contextMock.Setup(x => x.Message).Returns(@event);

        var feedback = new Feedback(Guid.CreateVersion7(), "John", "Doe", "Great book", 5);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _repositoryMock.Verify(x => x.Delete(feedback), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenFeedbackNotFound_WhenConsuming_ThenShouldReturnWithoutDeleting()
    {
        // Arrange
        var feedbackId = Guid.CreateVersion7();
        var @event = new BookUpdatedRatingFailedIntegrationEvent(feedbackId);
        _contextMock.Setup(x => x.Message).Returns(@event);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _repositoryMock.Verify(x => x.Delete(It.IsAny<Feedback>()), Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
