using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using MassTransit;

namespace BookWorm.Catalog.UnitTests.IntegrationEvents;

public sealed class FeedbackDeletedIntegrationEventHandlerTests
{
    private readonly Mock<ConsumeContext<FeedbackDeletedIntegrationEvent>> _contextMock = new();
    private readonly FeedbackDeletedIntegrationEventHandler _handler;
    private readonly Mock<IBookRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public FeedbackDeletedIntegrationEventHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenBookExists_WhenConsuming_ThenShouldRemoveRatingAndSave()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 4;

        var @event = new FeedbackDeletedIntegrationEvent(bookId, rating, feedbackId);
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
    public async Task GivenBookNotFound_WhenConsuming_ThenShouldReturnWithoutSaving()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 2;

        var @event = new FeedbackDeletedIntegrationEvent(bookId, rating, feedbackId);
        _contextMock.Setup(x => x.Message).Returns(@event);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
