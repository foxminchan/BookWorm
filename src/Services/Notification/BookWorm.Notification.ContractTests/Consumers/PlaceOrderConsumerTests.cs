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
    private readonly MailKitSettings _mailKitSettings;
    private readonly Mock<IRenderer> _rendererMock;
    private readonly Mock<ISender> _senderMock;

    public PlaceOrderConsumerTests()
    {
        _senderMock = new();
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _rendererMock = new();
        _rendererMock
            .Setup(x => x.Render(It.IsAny<Order>(), It.IsAny<string>()))
            .Returns("Rendered order content");

        _mailKitSettings = new() { From = "bookworm@example.com" };
    }

    private async Task<ITestHarness> CreateTestHarnessAsync()
    {
        var services = new ServiceCollection();

        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderCommandHandler>());

        services.AddScoped(_ => _senderMock.Object);
        services.AddSingleton(_rendererMock.Object);
        services.AddSingleton(_mailKitSettings);

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();
        return harness;
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
        var harness = await CreateTestHarnessAsync();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<PlaceOrderCommand>();

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
        var harness = await CreateTestHarnessAsync();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<PlaceOrderCommand>();

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
        var harness = await CreateTestHarnessAsync();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<PlaceOrderCommand>();

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
}
