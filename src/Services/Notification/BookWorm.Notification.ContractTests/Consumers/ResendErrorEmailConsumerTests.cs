using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class ResendErrorEmailConsumerTests : SnapshotTestBase
{
    private readonly Mock<GlobalLogBuffer> _logBufferMock = new();
    private readonly Mock<ILogger<ResendErrorEmailIntegrationEventHandler>> _loggerMock = new();
    private readonly Mock<IOutboxRepository> _repositoryMock = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenEmailsExist_WhenHandlingResendEvent_ThenShouldResendAllEmails()
    {
        // Arrange
        List<Outbox> failedEmails =
        [
            new("User One", "user1@example.com", "Subject 1", "Body 1"),
            new("User Two", "user2@example.com", "Subject 2", "Body 2"),
            new("User Three", "user3@example.com", "Subject 3", "Body 3"),
        ];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        var command = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<ResendErrorEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .AddScoped(_ => _senderMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<ResendErrorEmailIntegrationEventHandler>();

            await VerifySnapshot(new { harness, consumer });

            _repositoryMock.Verify(
                x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3)
            );

            VerifyLogMessage(
                LogLevel.Debug,
                "Successfully resent email to user1@example.com",
                Times.Once()
            );
            VerifyLogMessage(
                LogLevel.Debug,
                "Successfully resent email to user2@example.com",
                Times.Once()
            );
            VerifyLogMessage(
                LogLevel.Debug,
                "Successfully resent email to user3@example.com",
                Times.Once()
            );

            // Verify summary log
            VerifyLogMessage(
                LogLevel.Information,
                "Email resend completed. Success: 3, Failed: 0, Total: 3",
                Times.Once()
            );

            // No flush should be called when all successes (no failures)
            _logBufferMock.Verify(x => x.Flush(), Times.Never);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenNoEmailsExist_WhenHandlingResendEvent_ThenShouldCompleteWithoutSending()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var command = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<ResendErrorEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .AddScoped(_ => _senderMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<ResendErrorEmailIntegrationEventHandler>();

            await VerifySnapshot(new { harness, consumer });

            _repositoryMock.Verify(
                x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            // Verify summary log
            VerifyLogMessage(
                LogLevel.Information,
                "Email resend completed. Success: 0, Failed: 0, Total: 0",
                Times.Once()
            );

            // No flush should be called when no successes
            _logBufferMock.Verify(x => x.Flush(), Times.Never);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenEmailSendingFails_WhenHandlingResendEvent_ThenShouldLogErrorAndContinue()
    {
        // Arrange
        List<Outbox> failedEmails =
        [
            new("User One", "user1@example.com", "Subject 1", "Body 1"),
            new("User Two", "user2@example.com", "Subject 2", "Body 2"),
        ];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Send failed"));

        var command = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<ResendErrorEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .AddScoped(_ => _senderMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<ResendErrorEmailIntegrationEventHandler>();

            await VerifySnapshot(new { harness, consumer });

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );

            VerifyLogMessage(
                LogLevel.Error,
                "Failed to resend email to user1@example.com",
                Times.Once()
            );
            VerifyLogMessage(
                LogLevel.Error,
                "Failed to resend email to user2@example.com",
                Times.Once()
            );

            // Verify summary log
            VerifyLogMessage(
                LogLevel.Information,
                "Email resend completed. Success: 0, Failed: 2, Total: 2",
                Times.Once()
            );

            // Flush should be called when there are failures
            _logBufferMock.Verify(x => x.Flush(), Times.Once);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenMixedSuccessAndFailure_WhenHandlingResendEvent_ThenShouldProcessAllEmails()
    {
        // Arrange
        List<Outbox> failedEmails =
        [
            new("User One", "user1@example.com", "Subject 1", "Body 1"),
            new("User Two", "user2@example.com", "Subject 2", "Body 2"),
            new("User Three", "user3@example.com", "Subject 3", "Body 3"),
        ];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        // Setup partial failure - first email succeeds, second fails, third succeeds
        _senderMock
            .SetupSequence(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new("Send failed"))
            .Returns(Task.CompletedTask);

        var command = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<ResendErrorEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .AddScoped(_ => _senderMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<ResendErrorEmailIntegrationEventHandler>();

            await VerifySnapshot(new { harness, consumer });

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3)
            );

            // Verify success logs (2 successes)
            VerifyLogMessage(LogLevel.Debug, "Successfully resent email", Times.Exactly(2));

            // Verify error log (1 failure)
            VerifyLogMessage(LogLevel.Error, "Failed to resend email", Times.Once());

            // Verify summary log
            VerifyLogMessage(
                LogLevel.Information,
                "Email resend completed. Success: 2, Failed: 1, Total: 3",
                Times.Once()
            );

            _logBufferMock.Verify(x => x.Flush(), Times.Once());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenValidEmails_WhenHandlingResendEvent_ThenShouldCreateCorrectMimeMessages()
    {
        // Arrange
        var testEmail = new Outbox("John Doe", "john@example.com", "Test Subject", "Test Body");
        List<Outbox> failedEmails = [testEmail];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        MimeMessage? capturedMessage = null;
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MimeMessage, CancellationToken>((message, _) => capturedMessage = message)
            .Returns(Task.CompletedTask);

        var command = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<ResendErrorEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .AddScoped(_ => _senderMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<ResendErrorEmailIntegrationEventHandler>();

            await VerifySnapshot(new { harness, consumer });

            capturedMessage.ShouldNotBeNull();
            capturedMessage.To.Mailboxes.First().Name.ShouldBe("John Doe");
            capturedMessage.To.Mailboxes.First().Address.ShouldBe("john@example.com");
            capturedMessage.Subject.ShouldBe("Test Subject");
            capturedMessage.HtmlBody.ShouldBe("Test Body");

            // Verify summary log
            VerifyLogMessage(
                LogLevel.Information,
                "Email resend completed. Success: 1, Failed: 0, Total: 1",
                Times.Once()
            );
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenCancellationRequested_WhenHandlingResendEvent_ThenShouldRespectCancellation()
    {
        // Arrange
        List<Outbox> failedEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var command = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<ResendErrorEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .AddScoped(_ => _senderMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<ResendErrorEmailIntegrationEventHandler>();

            await VerifySnapshot(new { harness, consumer });

            VerifyLogMessage(LogLevel.Error, "Failed to resend email", Times.Once());

            // Verify summary log
            VerifyLogMessage(
                LogLevel.Information,
                "Email resend completed. Success: 0, Failed: 1, Total: 1",
                Times.Once()
            );

            // Flush should be called when there are failures
            _logBufferMock.Verify(x => x.Flush(), Times.Once);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenValidEmails_WhenHandlingResendEvent_ThenShouldPropagateCancellationTokenCorrectly()
    {
        // Arrange
        List<Outbox> failedEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        var capturedToken = CancellationToken.None;
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MimeMessage, CancellationToken>((_, token) => capturedToken = token)
            .Returns(Task.CompletedTask);

        var command = new ResendErrorEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<ResendErrorEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .AddScoped(_ => _senderMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command, capturedToken);

            // Assert
            var consumer = harness.GetConsumerHarness<ResendErrorEmailIntegrationEventHandler>();

            await VerifySnapshot(new { harness, consumer });

            capturedToken.ShouldNotBe(CancellationToken.None);
            capturedToken.CanBeCanceled.ShouldBeTrue();

            // Verify summary log
            VerifyLogMessage(
                LogLevel.Information,
                "Email resend completed. Success: 1, Failed: 0, Total: 1",
                Times.Once()
            );
        }
        finally
        {
            await harness.Stop(capturedToken);
        }
    }

    private void VerifyLogMessage(LogLevel level, string message, Times times)
    {
        _loggerMock.Verify(
            x =>
                x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            times
        );
    }
}
