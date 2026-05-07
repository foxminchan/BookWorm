using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Catalog.UnitTests.IntegrationEvents;

public sealed class FeedbackCreatedIntegrationEventHandlerTests
{
    private readonly Mock<IMessageBus> _busMock = new();
    private readonly FeedbackCreatedIntegrationEventHandler _handler;
    private readonly Mock<IBookRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public FeedbackCreatedIntegrationEventHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_repositoryMock.Object, _busMock.Object);
    }

    [Test]
    public async Task GivenBookExists_WhenHandling_ThenShouldAddRatingAndSave()
    {
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 5;

        var @event = new FeedbackCreatedIntegrationEvent(bookId, rating, feedbackId);

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

        await _handler.Handle(@event, CancellationToken.None);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _busMock.Verify(
            x =>
                x.PublishAsync(
                    It.IsAny<BookUpdatedRatingFailedIntegrationEvent>(),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenBookNotFound_WhenHandling_ThenShouldPublishFailedEventAndSave()
    {
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 3;

        var @event = new FeedbackCreatedIntegrationEvent(bookId, rating, feedbackId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.Handle(@event, CancellationToken.None);

        _busMock.Verify(
            x =>
                x.PublishAsync(
                    It.Is<BookUpdatedRatingFailedIntegrationEvent>(e => e.FeedbackId == feedbackId),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
