using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class ResendErrorEmailConsumerTests
{
    private Mock<GlobalLogBuffer> _logBufferMock = null!;
    private Mock<ILogger<ResendErrorEmailIntegrationEventHandler>> _loggerMock = null!;
    private Mock<IOutboxRepository> _repositoryMock = null!;
    private Mock<ISender> _senderMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private ResendErrorEmailIntegrationEventHandler _handler = null!;

    [Before(Test)]
    public void SetUp()
    {
        _logBufferMock = new();
        _loggerMock = new();
        _repositoryMock = new();
        _senderMock = new();
        _unitOfWorkMock = new();

        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object,
            _senderMock.Object
        );
    }

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
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        var command = new ResendErrorEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()),
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

    [Test]
    public async Task GivenNoEmailsExist_WhenHandlingResendEvent_ThenShouldCompleteWithoutSending()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var command = new ResendErrorEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        // Handler returns early via the debug log; summary log is never emitted
        VerifyLogMessage(LogLevel.Debug, "No unsent emails found for resend", Times.Once());

        _logBufferMock.Verify(x => x.Flush(), Times.Never);
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
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Send failed"));

        var command = new ResendErrorEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

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
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        // Setup partial failure - first email succeeds, second fails, third succeeds
        _senderMock
            .SetupSequence(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new("Send failed"))
            .Returns(Task.CompletedTask);

        var command = new ResendErrorEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

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

    [Test]
    public async Task GivenValidEmails_WhenHandlingResendEvent_ThenShouldCreateCorrectMimeMessages()
    {
        // Arrange
        var testEmail = new Outbox("John Doe", "john@example.com", "Test Subject", "Test Body");
        List<Outbox> failedEmails = [testEmail];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        MimeMessage? capturedMessage = null;
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MimeMessage, CancellationToken>((message, _) => capturedMessage = message)
            .Returns(Task.CompletedTask);

        var command = new ResendErrorEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

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

    [Test]
    public async Task GivenCancellationRequested_WhenHandlingResendEvent_ThenShouldRespectCancellation()
    {
        // Arrange
        List<Outbox> failedEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var command = new ResendErrorEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

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

    [Test]
    public async Task GivenValidEmails_WhenHandlingResendEvent_ThenShouldPropagateCancellationTokenCorrectly()
    {
        // Arrange
        List<Outbox> failedEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        CancellationToken capturedToken = default;
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MimeMessage, CancellationToken>((_, ct) => capturedToken = ct)
            .Returns(Task.CompletedTask);

        var command = new ResendErrorEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, token);

        // Assert
        await SnapshotTestHelper.Verify(command);

        capturedToken.ShouldBe(token);
        capturedToken.CanBeCanceled.ShouldBeTrue();

        // Verify summary log
        VerifyLogMessage(
            LogLevel.Information,
            "Email resend completed. Success: 1, Failed: 0, Total: 1",
            Times.Once()
        );
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
