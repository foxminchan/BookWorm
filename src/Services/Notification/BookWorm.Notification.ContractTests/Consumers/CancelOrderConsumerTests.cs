using BookWorm.Chassis.EventBus.Serialization;
using BookWorm.Common;
using BookWorm.Contracts;
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

public sealed class CancelOrderConsumerTests
{
    private const string Email = "test@example.com";
    private const string FullName = "Test User";
    private const decimal TotalMoney = 99.99m;
    private ITestHarness _harness = null!;
    private MailKitSettings _mailKitSettings = null!;
    private Guid _orderId;
    private ServiceProvider _provider = null!;
    private Mock<IRenderer> _rendererMock = null!;
    private Mock<ISender> _senderMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
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

        _mailKitSettings = new() { From = "bookworm@example.com" };

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<CancelOrderCommandHandler>();
                x.UsingInMemory(
                    (context, cfg) =>
                    {
                        cfg.UseCloudEvents();
                        cfg.ConfigureEndpoints(context);
                    }
                );
            })
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
    public async Task GivenValidCancelOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId, FullName, Email, TotalMoney);

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<CancelOrderCommandHandler>();
        await consumer.Consumed.Any<CancelOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

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
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<CancelOrderCommandHandler>();
        await consumer.Consumed.Any<CancelOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

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
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<CancelOrderCommandHandler>();
        await consumer.Consumed.Any<CancelOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
