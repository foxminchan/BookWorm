using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MimeKit;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class PlaceOrderConsumerTests
{
    private Mock<IRenderer> _rendererMock = null!;
    private Mock<ISender> _senderMock = null!;
    private PlaceOrderCommandHandler _handler = null!;

    [Before(Test)]
    public void SetUp()
    {
        _senderMock = new();
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _rendererMock = new();
        _rendererMock
            .Setup(x =>
                x.RenderAsync(It.IsAny<Order>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync("Rendered order content");

        _handler = new(_senderMock.Object, _rendererMock.Object);
    }

    [Test]
    public async Task GivenValidPlaceOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            "John Doe",
            "john.doe@example.com",
            Guid.CreateVersion7(),
            99.99m
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenPlaceOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            "John Doe",
            null,
            Guid.CreateVersion7(),
            99.99m
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenPlaceOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            "John Doe",
            string.Empty,
            Guid.CreateVersion7(),
            99.99m
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
