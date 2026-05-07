using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.IntegrationEvents.EventHandlers;

namespace BookWorm.Rating.ContractTests.Consumers;

public sealed class BookUpdatedRatingFailedConsumerTests
{
    private Guid _feedbackId;
    private Mock<IFeedbackRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public void SetUp()
    {
        _feedbackId = Guid.CreateVersion7();

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GivenValidFailedRatingEvent_WhenHandling_ThenShouldDeleteFeedback()
    {
        // Arrange
        var feedback = new Feedback(_feedbackId, "John", "Doe", "Great book!", 4);
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);

        var @event = new BookUpdatedRatingFailedIntegrationEvent(_feedbackId);
        var handler = new BookUpdatedRatingFailedIntegrationEventHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(@event);
        _repositoryMock.Verify(
            x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.Delete(feedback), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenFeedbackNotFound_WhenHandlingFailedRatingEvent_ThenShouldDoNothing()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        var @event = new BookUpdatedRatingFailedIntegrationEvent(_feedbackId);
        var handler = new BookUpdatedRatingFailedIntegrationEventHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(@event);
        _repositoryMock.Verify(
            x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.Delete(It.IsAny<Feedback>()), Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
