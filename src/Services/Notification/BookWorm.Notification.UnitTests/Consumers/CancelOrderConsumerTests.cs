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

public sealed class CancelOrderConsumerTests
{
    private readonly string _email;
    private readonly EmailOptions _emailOptions;
    private readonly string _fullName;
    private readonly Guid _orderId;
    private readonly Mock<IRenderer> _rendererMock;
    private readonly Mock<ISender> _senderMock;
    private readonly decimal _totalMoney;

    public CancelOrderConsumerTests()
    {
        _orderId = Guid.CreateVersion7();
        _email = "test@example.com";
        _totalMoney = 99.99m;
        _fullName = "Test User";

        _senderMock = new();
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _rendererMock = new();
        _rendererMock
            .Setup(x => x.Render(It.IsAny<Order>(), It.IsAny<string>()))
            .Returns("Rendered order content");

        _emailOptions = new() { From = "bookworm@example.com" };
    }

    [Test]
    public async Task GivenValidCancelOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId, _fullName, _email, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CancelOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CancelOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CancelOrderCommand>()).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenCancelOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId, _fullName, null, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CancelOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CancelOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CancelOrderCommand>()).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenCancelOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId, _fullName, string.Empty, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CancelOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddScoped(_ => _rendererMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CancelOrderCommandHandler>();
        (await consumerHarness.Consumed.Any<CancelOrderCommand>()).ShouldBeTrue();

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }
}
