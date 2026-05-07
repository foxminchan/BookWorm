using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.Repository;
using BookWorm.Contracts;

namespace BookWorm.Catalog.UnitTests.IntegrationEvents;

public sealed class FeedbackDeletedIntegrationEventHandlerTests
{
    private readonly FeedbackDeletedIntegrationEventHandler _handler;
    private readonly Mock<IBookRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public FeedbackDeletedIntegrationEventHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenBookExists_WhenHandling_ThenShouldRemoveRatingAndSave()
    {
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 4;

        var @event = new FeedbackDeletedIntegrationEvent(bookId, rating, feedbackId);

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
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenBookNotFound_WhenHandling_ThenShouldReturnWithoutSaving()
    {
        var bookId = Guid.CreateVersion7();
        var feedbackId = Guid.CreateVersion7();
        const int rating = 2;

        var @event = new FeedbackDeletedIntegrationEvent(bookId, rating, feedbackId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        await _handler.Handle(@event, CancellationToken.None);

        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
