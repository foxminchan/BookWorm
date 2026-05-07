using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Exceptions;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Builders;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MimeKit;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class CompleteOrderConsumerTests
{
    private const decimal TotalMoney = 150.99m;
    private const string FullName = "John Doe";
    private const string ValidEmail = "customer@example.com";
    private Guid _orderId;
    private Mock<IRenderer> _rendererMock = null!;
    private Mock<ISender> _senderMock = null!;

    [Before(Test)]
    public void SetUp()
    {
        _orderId = Guid.CreateVersion7();
        _senderMock = new();
        _rendererMock = new();

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _rendererMock
            .Setup(x =>
                x.RenderAsync(It.IsAny<Order>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync("Rendered order content");
    }

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, ValidEmail, TotalMoney);
        var handler = new CompleteOrderCommandHandler(_senderMock.Object, _rendererMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, string.Empty, TotalMoney);
        var handler = new CompleteOrderCommandHandler(_senderMock.Object, _rendererMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, null, TotalMoney);
        var handler = new CompleteOrderCommandHandler(_senderMock.Object, _rendererMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenSmtpClientThrows_ThenShouldPropagateException()
    {
        // Arrange
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Failed to send email"));

        var command = new CompleteOrderCommand(_orderId, FullName, ValidEmail, TotalMoney);
        var handler = new CompleteOrderCommandHandler(_senderMock.Object, _rendererMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await handler.Handle(command, CancellationToken.None)
        );

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public void GivenOrderWithInvalidStatus_WhenHandling_ThenShouldThrowNotificationException()
    {
        // Arrange
        const Status invalidStatus = (Status)99;
        var order = new Order(_orderId, FullName, TotalMoney, invalidStatus);
        var builder = OrderMimeMessageBuilder.Initialize();

        // Act & Assert
        var exception = Assert.Throws<NotificationException>(() => builder.WithSubject(order));
        exception?.Message.ShouldBe($"Invalid status: {invalidStatus}");
    }
}
