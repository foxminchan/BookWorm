using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MimeKit;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class CancelOrderConsumerTests
{
    private const string Email = "test@example.com";
    private const string FullName = "Test User";
    private const decimal TotalMoney = 99.99m;
    private Guid _orderId;
    private Mock<IRenderer> _rendererMock = null!;
    private Mock<ISender> _senderMock = null!;
    private CancelOrderCommandHandler _handler = null!;

    [Before(Test)]
    public void SetUp()
    {
        _orderId = Guid.CreateVersion7();

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
    public async Task GivenValidCancelOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId, FullName, Email, TotalMoney);

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
    public async Task GivenCancelOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId, FullName, null, TotalMoney);

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
    public async Task GivenCancelOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId, FullName, string.Empty, TotalMoney);

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
