using BookWorm.Contracts;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Handlers;

public sealed class PlaceOrderCommandHandlerTests
{
    private readonly Mock<ConsumeContext<PlaceOrderCommand>> _contextMock = new();
    private readonly PlaceOrderCommandHandler _handler;
    private readonly Mock<IRenderer> _rendererMock = new();
    private readonly Mock<ISender> _senderMock = new();

    public PlaceOrderCommandHandlerTests()
    {
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_senderMock.Object, _rendererMock.Object);
    }

    [Test]
    public async Task GivenValidEmail_WhenConsuming_ThenShouldRenderAndSendEmail()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            "Bob Wilson",
            "bob@example.com",
            Guid.CreateVersion7(),
            200.00m
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
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            "Bob Wilson",
            email,
            Guid.CreateVersion7(),
            200.00m
        );

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
