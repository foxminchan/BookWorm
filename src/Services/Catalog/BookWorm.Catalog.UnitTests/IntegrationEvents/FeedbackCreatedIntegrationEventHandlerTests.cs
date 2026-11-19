using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Chassis.Repository;
using BookWorm.Contracts;

namespace BookWorm.Catalog.UnitTests.IntegrationEvents;

public sealed class FeedbackCreatedIntegrationEventHandlerTests
{
    private readonly FeedbackCreatedIntegrationEventHandler _handler;
    private readonly Mock<IBookRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public FeedbackCreatedIntegrationEventHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_repositoryMock.Object);
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
            CategoryId.From(Guid.CreateVersion7()),
            PublisherId.From(Guid.CreateVersion7()),
            [AuthorId.From(Guid.CreateVersion7())]
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(@event, CancellationToken.None);

        result.ShouldBeNull();
        _repositoryMock.Verify(
            x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenBookNotFound_WhenHandling_ThenShouldReturnFailedEventAndSave()
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

        var result = await _handler.Handle(@event, CancellationToken.None);

        result.ShouldNotBeNull();
        result.FeedbackId.ShouldBe(feedbackId);
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenDuplicateEvent_WhenHandling_ThenShouldIgnoreWithoutMutatingBook()
    {
        var @event = new FeedbackCreatedIntegrationEvent(
            Guid.CreateVersion7(),
            5,
            Guid.CreateVersion7()
        );

        var guardMock = new Mock<IIntegrationEventIdempotencyGuard>();
        guardMock
            .Setup(x =>
                x.TryAcquireAsync(
                    @event.Id,
                    nameof(FeedbackCreatedIntegrationEventHandler),
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        var handler = new FeedbackCreatedIntegrationEventHandler(
            _repositoryMock.Object,
            guardMock.Object
        );

        var result = await handler.Handle(@event, CancellationToken.None);

        result.ShouldBeNull();
        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
