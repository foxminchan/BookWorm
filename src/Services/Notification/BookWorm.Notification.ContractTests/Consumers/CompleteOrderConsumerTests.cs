using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Builders;
using BookWorm.Notification.Domain.Exceptions;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class CompleteOrderConsumerTests
{
    private const decimal TotalMoney = 150.99m;
    private const string FullName = "John Doe";
    private const string ValidEmail = "customer@example.com";
    private Guid _orderId;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private MailKitSettings _mailKitSettings = null!;
    private Mock<IRenderer> _rendererMock = null!;
    private Mock<ISender> _senderMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _orderId = Guid.CreateVersion7();
        _mailKitSettings = new() { From = "store@bookworm.com" };
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

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _mailKitSettings)
            .BuildServiceProvider(true);

        _harness = await _provider.StartTestHarness();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, ValidEmail, TotalMoney);

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        await consumer.Consumed.Any<CompleteOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

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

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        await consumer.Consumed.Any<CompleteOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

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

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        await consumer.Consumed.Any<CompleteOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenSmtpClientThrows_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, ValidEmail, TotalMoney);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to send email"));

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumerHarness = _harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        var consumeContext = _harness.Consumed.Select<CompleteOrderCommand>().First();
        (
            await _harness.Consumed.Any<CompleteOrderCommand>(x =>
                x.Context.MessageId == consumeContext.Context.MessageId
            )
        ).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenOrderWithInvalidStatus_WhenHandling_ThenShouldThrowNotificationException()
    {
        // Arrange
        const Status invalidStatus = (Status)99; // Invalid status value
        var order = new Order(_orderId, FullName, TotalMoney, invalidStatus);

        _rendererMock
            .Setup(x =>
                x.RenderAsync(
                    It.Is<Order>(o => o.Status == invalidStatus),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("Rendered order content");

        // Create a message builder and test it directly
        var builder = OrderMimeMessageBuilder.Initialize();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotificationException>(() =>
        {
            builder.WithSubject(order);
            return Task.CompletedTask;
        });

        exception?.Message.ShouldBe($"Invalid status: {invalidStatus}");

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
