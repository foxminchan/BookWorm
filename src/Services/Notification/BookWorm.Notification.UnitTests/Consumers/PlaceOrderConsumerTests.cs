﻿using System.Net.Mail;
using BookWorm.Contracts;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Notification.UnitTests.Consumers;

public sealed class PlaceOrderConsumerTests
{
    private readonly Guid _basketId;
    private readonly string _email;
    private readonly EmailOptions _emailOptions;
    private readonly Guid _orderId;
    private readonly Mock<ISmtpClient> _smtpClientMock;
    private readonly decimal _totalMoney;

    public PlaceOrderConsumerTests()
    {
        _orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();
        _email = "test@example.com";
        _totalMoney = 99.99m;

        _smtpClientMock = new();
        _smtpClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailOptions = new() { From = "bookworm@example.com" };
    }

    [Test]
    public async Task GivenValidPlaceOrderCommand_WhenHandling_ThenShouldSendEmail()
    {
        // Arrange
        var command = new PlaceOrderCommand(_basketId, _email, _orderId, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped<ISmtpClient>(_ => _smtpClientMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();

        _smtpClientMock.Verify(
            x =>
                x.SendEmailAsync(
                    It.Is<MailMessage>(m =>
                        m.To.Contains(new(_email))
                        && m.Subject == "Your order has been placed"
                        && m.Body.Contains(_orderId.ToString())
                        && m.Body.Contains(_totalMoney.ToString("C"))
                        && m.IsBodyHtml == true
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenPlaceOrderCommandWithNullEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new PlaceOrderCommand(_basketId, null, _orderId, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped<ISmtpClient>(_ => _smtpClientMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();

        _smtpClientMock.Verify(
            x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenPlaceOrderCommandWithEmptyEmail_WhenHandling_ThenShouldNotSendEmail()
    {
        // Arrange
        var command = new PlaceOrderCommand(_basketId, string.Empty, _orderId, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped<ISmtpClient>(_ => _smtpClientMock.Object)
            .AddSingleton(_ => _emailOptions)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();

        _smtpClientMock.Verify(
            x => x.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }
}
