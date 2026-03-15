using BookWorm.Contracts;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookWorm.Ordering.UnitTests.IntegrationEvents;

public sealed class DeleteBasketCompleteCommandHandlerTests
{
    private readonly Mock<ConsumeContext<DeleteBasketCompleteCommand>> _contextMock = new();
    private readonly DeleteBasketCompleteCommandHandler _handler;
    private readonly Mock<ILogger<DeleteBasketCompleteCommandHandler>> _loggerMock = new();

    public DeleteBasketCompleteCommandHandlerTests()
    {
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_loggerMock.Object);
    }

    [Test]
    public async Task GivenValidCommand_WhenConsuming_ThenShouldLogAndComplete()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(Guid.CreateVersion7(), 199.99m);
        _contextMock.Setup(x => x.Message).Returns(command);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert - handler should complete without throwing
        _contextMock.Verify(x => x.Message, Times.Once);
    }
}
