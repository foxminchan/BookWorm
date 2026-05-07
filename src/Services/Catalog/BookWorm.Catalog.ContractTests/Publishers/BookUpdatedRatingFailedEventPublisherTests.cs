using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Catalog.ContractTests.Publishers;

public sealed class BookUpdatedRatingFailedEventPublisherTests
{
    private readonly int _rating = 4;
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

        var @event = new FeedbackCreatedIntegrationEvent(_bookId, _rating, _feedbackId);
        var bus = new TestMessageContext();
        var handler = new FeedbackCreatedIntegrationEventHandler(_repositoryMock.Object, bus);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvents(
            bus.AllOutgoing.Select(e => ((Envelope)e).Message).ToList()
        );
        _repositoryMock.Verify(
            x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
