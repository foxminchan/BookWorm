using BookWorm.Chassis.EventBus.Dispatcher;
using BookWorm.Rating.Domain.EventHandlers;
using BookWorm.Rating.Domain.Events;

namespace BookWorm.Rating.UnitTests.Domain.EventHandlers;

public sealed class FeedbackDeletedEventHandlerTests
{
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();
    private readonly FeedbackDeletedEventHandler _handler;

    public FeedbackDeletedEventHandlerTests()
    {
        _handler = new(_eventDispatcherMock.Object);
    }

    [Test]
    public async Task GivenFeedbackDeletedEvent_WhenHandling_ThenShouldDispatchEvent()
    {
        // Arrange
        var notification = new FeedbackDeletedEvent(
            Guid.CreateVersion7(),
            4,
            Guid.CreateVersion7()
        );

        _eventDispatcherMock
            .Setup(x => x.DispatchAsync(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _eventDispatcherMock.Verify(
            x => x.DispatchAsync(notification, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
