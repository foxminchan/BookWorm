using BookWorm.Chassis.EventBus.Dispatcher;
using BookWorm.Rating.Domain.EventHandlers;
using BookWorm.Rating.Domain.Events;

namespace BookWorm.Rating.UnitTests.Domain.EventHandlers;

public sealed class FeedbackCreatedEventHandlerTests
{
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();
    private readonly FeedbackCreatedEventHandler _handler;

    public FeedbackCreatedEventHandlerTests()
    {
        _handler = new(_eventDispatcherMock.Object);
    }

    [Test]
    public async Task GivenFeedbackCreatedEvent_WhenHandling_ThenShouldDispatchEvent()
    {
        // Arrange
        var notification = new FeedbackCreatedEvent(
            Guid.CreateVersion7(),
            5,
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
