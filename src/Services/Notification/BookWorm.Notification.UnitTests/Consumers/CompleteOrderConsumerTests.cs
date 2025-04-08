using System.Net.Mail;
using BookWorm.Contracts;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Notification.UnitTests.Consumers;

public sealed class CompleteOrderConsumerTests
{
    private const decimal TotalMoney = 150.99m;
    private const string ValidEmail = "customer@example.com";
    private readonly EmailOptions _emailOptions = new() { From = "store@bookworm.com" };
    private readonly Guid _orderId = Guid.CreateVersion7();
    private readonly Mock<ISmtpClient> _smtpClientMock = new();

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, ValidEmail, TotalMoney);

        _smtpClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped<ISmtpClient>(_ => _smtpClientMock.Object)
            .AddScoped(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CompleteOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        _smtpClientMock.Verify(
            x =>
                x.SendEmailAsync(
                    It.Is<MailMessage>(m =>
                        m.To.Contains(new(ValidEmail))
                        && m.From!.Address == _emailOptions.From
                        && m.Subject == "Your order has been completed"
                        && m.Body.Contains(_orderId.ToString())
                        && m.Body.Contains(TotalMoney.ToString("C"))
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, string.Empty, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped<ISmtpClient>(_ => _smtpClientMock.Object)
            .AddScoped(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CompleteOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        _smtpClientMock.Verify(
            x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenCompleteOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, null, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped<ISmtpClient>(_ => _smtpClientMock.Object)
            .AddScoped(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CompleteOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<CompleteOrderCommand>()).ShouldBeTrue();

        _smtpClientMock.Verify(
            x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidCompleteOrderCommand_WhenSmtpClientThrows_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId, ValidEmail, TotalMoney);
        var expectedException = new SmtpException("Failed to send email");

        _smtpClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<CompleteOrderCommandHandler>())
            .AddScoped<ISmtpClient>(_ => _smtpClientMock.Object)
            .AddScoped(_ => _emailOptions)
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

        _smtpClientMock.Verify(
            x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        await harness.Stop();
    }
}
