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
    private BookUpdatedRatingFailedIntegrationEventHandler _handler = null!;

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

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenValidFailedRatingEvent_WhenHandling_ThenShouldDeleteFeedback()
    {
        // Arrange
        var feedback = new Feedback(_feedbackId, "John", "Doe", "Great book!", 4);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);

        var integrationEvent = new BookUpdatedRatingFailedIntegrationEvent(_feedbackId);

        // Act
        await _handler.Handle(integrationEvent, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(integrationEvent);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(feedback), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenFailedRatingEventWithNonExistingFeedback_WhenHandling_ThenShouldReturnGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        var integrationEvent = new BookUpdatedRatingFailedIntegrationEvent(_feedbackId);

        // Act
        await _handler.Handle(integrationEvent, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(integrationEvent);

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
