using BookWorm.Contracts;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Handlers;

public sealed class CancelOrderCommandHandlerTests
{
    private readonly Mock<ConsumeContext<CancelOrderCommand>> _contextMock = new();
    private readonly CancelOrderCommandHandler _handler;
    private readonly Mock<IRenderer> _rendererMock = new();
    private readonly Mock<ISender> _senderMock = new();

    public CancelOrderCommandHandlerTests()
    {
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_senderMock.Object, _rendererMock.Object);
    }

    [Test]
    public async Task GivenValidEmail_WhenConsuming_ThenShouldRenderAndSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.CreateVersion7(),
            "John Doe",
            "john@example.com",
            99.99m
        );

        _contextMock.Setup(x => x.Message).Returns(command);

        _rendererMock
            .Setup(x =>
                x.RenderAsync(
                    It.IsAny<object>(),
                    "Orders/OrderEmail",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("<html>rendered</html>");

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
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
    public async Task GivenNullOrEmptyEmail_WhenConsuming_ThenShouldReturnWithoutSending(
        string? email
    )
    {
        // Arrange
        var command = new CancelOrderCommand(Guid.CreateVersion7(), "John Doe", email, 99.99m);

        _contextMock.Setup(x => x.Message).Returns(command);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
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
