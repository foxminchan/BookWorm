using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using BookWorm.Notification.UnitTests.Fakers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Consumers;

public sealed class PlaceOrderConsumerTests : SnapshotTestBase
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

        _mailKitSettings = TestDataFakers.EmailOptions.Generate();
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
        var command = TestDataFakers.PlaceOrderCommand.Generate();
        var harness = await CreateTestHarnessAsync();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();
            (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

            // Contract verification - using deterministic data for snapshot consistency
            var contractCommand = new PlaceOrderCommand(
                Guid.CreateVersion7(), // BasketId
                "John Doe", // FullName
                "john.doe@example.com", // Email
                Guid.CreateVersion7(), // OrderId
                99.99m // TotalMoney
            );
            await VerifySnapshot(contractCommand);
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
        var command = FakerExtensions.WithNullEmail();
        var harness = await CreateTestHarnessAsync();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();
            (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            // Contract verification - using deterministic data for snapshot consistency
            var contractCommand = new PlaceOrderCommand(
                Guid.CreateVersion7(), // BasketId
                "John Doe", // FullName
                null, // Email (null for this test)
                Guid.CreateVersion7(), // OrderId
                99.99m // TotalMoney
            );
            await VerifySnapshot(contractCommand);
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
        var command = FakerExtensions.WithEmptyEmailAddress();
        var harness = await CreateTestHarnessAsync();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();
            (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            // Contract verification - using deterministic data for snapshot consistency
            var contractCommand = new PlaceOrderCommand(
                Guid.CreateVersion7(), // BasketId
                "John Doe", // FullName
                "", // Email (empty for this test)
                Guid.CreateVersion7(), // OrderId
                99.99m // TotalMoney
            );
            await VerifySnapshot(contractCommand);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
