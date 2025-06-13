using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Domain.Settings;
using BookWorm.Notification.Infrastructure.Render;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using BookWorm.Notification.UnitTests.Fakers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Consumers;

public sealed class PlaceOrderConsumerTests
{
    private readonly EmailOptions _emailOptions;
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

        _emailOptions = TestDataFakers.EmailOptions.Generate();
    }

    private async Task<ITestHarness> CreateTestHarnessAsync()
    {
        var services = new ServiceCollection();

        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<PlaceOrderCommandHandler>());

        services.AddScoped(_ => _senderMock.Object);
        services.AddSingleton(_rendererMock.Object);
        services.AddSingleton(_emailOptions);

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
        var command = TestDataFakers.PlaceOrderCommand.WithNullEmail();
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
        var command = TestDataFakers.PlaceOrderCommand.WithEmptyEmailAddress();
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
        }
        finally
        {
            await harness.Stop();
        }
    }
}
