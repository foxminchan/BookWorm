using BookWorm.Contracts;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Handlers;

public sealed class CancelOrderCommandHandlerTests
{
    private readonly CancelOrderCommandHandler _handler;
    private readonly Mock<IRenderer> _rendererMock = new();
    private readonly Mock<ISender> _senderMock = new();

    public CancelOrderCommandHandlerTests()
    {
        _handler = new(_senderMock.Object, _rendererMock.Object);
    }

    [Test]
    public async Task GivenValidEmail_WhenHandling_ThenShouldRenderAndSendEmail()
    {
        var command = new CancelOrderCommand(
            Guid.CreateVersion7(),
            "John Doe",
            "john@example.com",
            99.99m
        );

        _rendererMock
            .Setup(x =>
                x.RenderAsync(
                    It.IsAny<object>(),
                    "Orders/OrderEmail",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("<html>rendered</html>");

        await _handler.Handle(command, CancellationToken.None);

        _rendererMock.Verify(
            x =>
                x.RenderAsync(
                    It.IsAny<object>(),
                    "Orders/OrderEmail",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task GivenNullOrEmptyEmail_WhenHandling_ThenShouldReturnWithoutSending(
        string? email
    )
    {
        var command = new CancelOrderCommand(Guid.CreateVersion7(), "John Doe", email, 99.99m);

        await _handler.Handle(command, CancellationToken.None);

        _rendererMock.Verify(
            x =>
                x.RenderAsync(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
