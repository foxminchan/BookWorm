using BookWorm.Contracts;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using Microsoft.Extensions.Logging;

namespace BookWorm.Ordering.UnitTests.IntegrationEvents;

public sealed class DeleteBasketCompleteCommandHandlerTests
{
    private readonly DeleteBasketCompleteCommandHandler _handler;
    private readonly Mock<ILogger<DeleteBasketCompleteCommandHandler>> _loggerMock = new();

    public DeleteBasketCompleteCommandHandlerTests()
    {
        _handler = new(_loggerMock.Object);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandling_ThenShouldCompleteWithoutThrowing()
    {
        var command = new DeleteBasketCompleteCommand(Guid.CreateVersion7(), 199.99m);

        await Should.NotThrowAsync(() => _handler.Handle(command, CancellationToken.None));
    }
}
