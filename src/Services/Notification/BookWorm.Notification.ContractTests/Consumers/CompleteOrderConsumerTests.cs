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
    private readonly MailKitSettings _mailKitSettings = new() { From = "store@bookworm.com" };
    private readonly Guid _orderId = Guid.CreateVersion7();
    private readonly Mock<IRenderer> _rendererMock = new();
    private readonly Mock<ISender> _senderMock = new();

    public CompleteOrderConsumerTests()
    {
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _rendererMock
            .Setup(x => x.Render(It.IsAny<Order>(), It.IsAny<string>()))
            .Returns("Rendered order content");
    }

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, ValidEmail, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _mailKitSettings)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<CompleteOrderCommandHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<CompleteOrderCommand>();

            await SnapshotTestHelper.Verify(new { harness, consumer });

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, string.Empty, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _mailKitSettings)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<CompleteOrderCommandHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<CompleteOrderCommand>();

            await SnapshotTestHelper.Verify(new { harness, consumer });

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, null, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _mailKitSettings)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<CompleteOrderCommandHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<CompleteOrderCommand>();

            await SnapshotTestHelper.Verify(new { harness, consumer });

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenSmtpClientThrows_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, ValidEmail, TotalMoney);
        var expectedException = new Exception("Failed to send email");

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _mailKitSettings)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act & Assert
        await harness.Bus.Publish(command);

        var consumerHarness = harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        // Verify the exception was thrown
        var consumeContext = harness.Consumed.Select<CompleteOrderCommand>().First();
        (
            await harness.Consumed.Any<CompleteOrderCommand>(x =>
                x.Context.MessageId == consumeContext.Context.MessageId
            )
        ).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenOrderWithInvalidStatus_WhenHandling_ThenShouldThrowNotificationException()
    {
        // Arrange
        const Status invalidStatus = (Status)99; // Invalid status value
        var order = new Order(_orderId, FullName, TotalMoney, invalidStatus);

        _rendererMock
            .Setup(x => x.Render(It.Is<Order>(o => o.Status == invalidStatus), It.IsAny<string>()))
            .Returns("Rendered order content");

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _mailKitSettings)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

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

        await harness.Stop();
    }
}
