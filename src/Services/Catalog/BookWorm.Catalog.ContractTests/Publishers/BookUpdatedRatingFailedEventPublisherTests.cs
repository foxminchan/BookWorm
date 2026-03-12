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
    private Mock<IMessageContext> _contextMock = null!;
    private List<object> _published = null!;
    private FeedbackCreatedIntegrationEventHandler _handler = null!;

    [Before(Test)]
    public void SetUp()
    {
        _bookId = Guid.CreateVersion7();
        _feedbackId = Guid.CreateVersion7();
        _published = [];

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _contextMock = new();
        _contextMock
            .Setup(x => x.PublishAsync(It.IsAny<object>(), null))
            .Callback<object, DeliveryOptions?>((msg, _) => _published.Add(msg))
            .Returns(ValueTask.CompletedTask);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenNonExistingBook_WhenHandlingFeedbackCreated_ThenShouldPublishFailedEvent()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var integrationEvent = new FeedbackCreatedIntegrationEvent(_bookId, _rating, _feedbackId);

        // Act
        await _handler.Handle(integrationEvent, _contextMock.Object, CancellationToken.None);

        // Assert
        var failedEvent = _published
            .OfType<BookUpdatedRatingFailedIntegrationEvent>()
            .ShouldHaveSingleItem();
        failedEvent.FeedbackId.ShouldBe(_feedbackId);

        await SnapshotTestHelper.Verify(new { @event = integrationEvent, published = _published });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
