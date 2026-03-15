using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using MassTransit;

namespace BookWorm.Catalog.UnitTests.IntegrationEvents;

public sealed class FeedbackCreatedIntegrationEventHandlerTests
{
    private readonly Mock<ConsumeContext<FeedbackCreatedIntegrationEvent>> _contextMock = new();
    private readonly FeedbackCreatedIntegrationEventHandler _handler;
    private readonly Mock<IBookRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public FeedbackCreatedIntegrationEventHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenBookExists_WhenConsuming_ThenShouldAddRatingAndSave()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 5;

        var @event = new FeedbackCreatedIntegrationEvent(bookId, rating, feedbackId);
        _contextMock.Setup(x => x.Message).Returns(@event);

        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            29.99m,
            24.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _repositoryMock.Verify(
            x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenBookNotFound_WhenConsuming_ThenShouldPublishFailedEventAndSave()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 3;

        var @event = new FeedbackCreatedIntegrationEvent(bookId, rating, feedbackId);
        _contextMock.Setup(x => x.Message).Returns(@event);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _contextMock.Verify(
            x =>
                x.Publish(
                    It.Is<BookUpdatedRatingFailedIntegrationEvent>(e => e.FeedbackId == feedbackId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
