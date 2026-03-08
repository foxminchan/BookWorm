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

public sealed class PlaceOrderConsumerTests
{
    private ITestHarness _harness = null!;
    private MailKitSettings _mailKitSettings = null!;
    private ServiceProvider _provider = null!;
    private Mock<IRenderer> _rendererMock = null!;
    private Mock<ISender> _senderMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
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

        _mailKitSettings = new() { From = "bookworm@example.com" };

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped(_ => _senderMock.Object)
            .AddSingleton(_rendererMock.Object)
            .AddSingleton(_mailKitSettings)
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
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<PlaceOrderCommandHandler>();
        await consumer.Consumed.Any<PlaceOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

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
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<PlaceOrderCommandHandler>();
        await consumer.Consumed.Any<PlaceOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

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
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<PlaceOrderCommandHandler>();
        await consumer.Consumed.Any<PlaceOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
