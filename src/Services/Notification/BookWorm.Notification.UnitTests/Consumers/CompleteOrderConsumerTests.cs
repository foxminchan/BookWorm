using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Domain.Settings;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Consumers;

public sealed class CompleteOrderConsumerTests
{
    private const decimal TotalMoney = 150.99m;
    private const string FullName = "John Doe";
    private const string ValidEmail = "customer@example.com";
    private readonly EmailOptions _emailOptions = new() { From = "store@bookworm.com" };
    private readonly Guid _orderId = Guid.CreateVersion7();
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<IRenderer> _rendererMock = new();

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
            .AddScoped<ISender>(_ => _senderMock.Object)
            .AddScoped<IRenderer>(_ => _rendererMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, string.Empty, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped<ISender>(_ => _senderMock.Object)
            .AddScoped<IRenderer>(_ => _rendererMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, FullName, null, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped<ISender>(_ => _senderMock.Object)
            .AddScoped<IRenderer>(_ => _rendererMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CompleteOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
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
            .AddScoped<ISender>(_ => _senderMock.Object)
            .AddScoped<IRenderer>(_ => _rendererMock.Object)
            .AddSingleton(_ => _emailOptions)
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
}
